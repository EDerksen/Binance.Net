﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.OrderBook;
using CryptoExchange.Net.Sockets;

namespace Binance.Net
{
    /// <summary>
    /// Implementation for a synchronized order book. After calling Start the order book will sync itself and keep up to date with new data. It will automatically try to reconnect and resync in case of a lost/interrupted connection.
    /// Make sure to check the State property to see if the order book is synced.
    /// </summary>
    public class BinanceSymbolOrderBook: SymbolOrderBook
    {
        private readonly BinanceClient restClient;
        private readonly BinanceSocketClient socketClient;
        private readonly int? limit;
        private bool initialUpdateReceived;

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="symbol">The symbol of the order book</param>
        /// <param name="options">The options for the order book</param>
        public BinanceSymbolOrderBook(string symbol, BinanceOrderBookOptions options = null) : base(symbol, options ?? new BinanceOrderBookOptions())
        {
            limit = options?.Limit;
            restClient = new BinanceClient();
            socketClient = new BinanceSocketClient();
        }

        protected override async Task<CallResult<UpdateSubscription>> DoStart()
        {
            if (limit.HasValue && limit != 5 && limit != 10 && limit != 20)
                return new CallResult<UpdateSubscription>(null, new ArgumentError("Limit should be one of the following: 5, 10, 20 or null for full order book"));

            CallResult<UpdateSubscription> subResult;
            if (limit == null)
                subResult = await socketClient.SubscribeToDepthStreamAsync(Symbol, HandleUpdate).ConfigureAwait(false);
            else
                subResult = await socketClient.SubscribeToPartialBookDepthStreamAsync(Symbol, limit.Value, HandleUpdate).ConfigureAwait(false);

            if (!subResult.Success)
                return new CallResult<UpdateSubscription>(null, subResult.Error);

            Status = OrderBookStatus.Syncing;
            if (limit == null)
            {
                var bookResult = await restClient.GetOrderBookAsync(Symbol, limit ?? 1000).ConfigureAwait(false);
                if (!bookResult.Success)
                {
                    await socketClient.UnsubscribeAll().ConfigureAwait(false);
                    return new CallResult<UpdateSubscription>(null, bookResult.Error);
                }

                SetInitialOrderBook(bookResult.Data.LastUpdateId, bookResult.Data.Asks, bookResult.Data.Bids);
            }
            else
            {
                while (!initialUpdateReceived && Status == OrderBookStatus.Syncing)
                    await Task.Delay(10).ConfigureAwait(false);
            }

            return new CallResult<UpdateSubscription>(subResult.Data, null);
        }

        private void HandleUpdate(BinanceOrderBook data)
        {
            if (limit == null)
            {
                var updates = new List<ProcessEntry>();
                updates.AddRange(data.Asks.Select(a => new ProcessEntry(OrderBookEntryType.Ask, a)));
                updates.AddRange(data.Bids.Select(b => new ProcessEntry(OrderBookEntryType.Bid, b)));
                UpdateOrderBook(data.FirstUpdateId ?? data.LastUpdateId, data.LastUpdateId, updates);
            }
            else
            {
                initialUpdateReceived = true;
                SetInitialOrderBook(data.LastUpdateId, data.Asks, data.Bids);
            }
        }

        protected override void DoReset()
        {
            initialUpdateReceived = false;
        }

        protected override async Task<CallResult<bool>> DoResync()
        {
            if (limit != null)
            {
                while (!initialUpdateReceived && Status == OrderBookStatus.Syncing)
                    await Task.Delay(10).ConfigureAwait(false);

                return new CallResult<bool>(true, null);
            }
            
            var bookResult = await restClient.GetOrderBookAsync(Symbol, limit).ConfigureAwait(false);
            if (!bookResult.Success)
                return new CallResult<bool>(false, bookResult.Error);

            SetInitialOrderBook(bookResult.Data.LastUpdateId, bookResult.Data.Asks, bookResult.Data.Bids);
            return new CallResult<bool>(true, null);
        }

        public override void Dispose()
        {
            processBuffer.Clear();
            asks.Clear();
            bids.Clear();

            restClient?.Dispose();
            socketClient?.Dispose();
        }
    }
}
