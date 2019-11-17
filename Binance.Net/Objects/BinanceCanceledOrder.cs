﻿using Binance.Net.Converters;
using Newtonsoft.Json;

namespace Binance.Net.Objects
{
    /// <summary>
    /// Information about a canceled order
    /// </summary>
    public class BinanceCanceledOrder
    {
        /// <summary>
        /// The symbol the order was for
        /// </summary>
        public string Symbol { get; set; } = "";
        /// <summary>
        /// The client order id the order was
        /// </summary>
        [JsonProperty("origClientOrderId")]
        public string OriginalClientOrderId { get; set; } = "";
        /// <summary>
        /// The original quote order quantity
        /// </summary>
        [JsonProperty("origQuoteOrderQty")]
        public decimal OriginalQuoteOrderQuantity { get; set; }
        /// <summary>
        /// The order id as generated by Binance
        /// </summary>
        public long OrderId { get; set; }
        /// <summary>
        /// The new client order id
        /// </summary>
        public string ClientOrderId { get; set; } = "";
        /// <summary>
        /// The order list id as generated by Binance, only for OCO orders
        /// </summary>
        public long OrderListId { get; set; }

        ///// <summary>
        ///// The time of the cancellation
        ///// </summary>
        //[JsonConverter(typeof(TimestampConverter))]
        //public DateTime TransactTime { get; set; }  NOTE: mentioned in documentation but not returned by API

        /// <summary>
        /// The price the order was for
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Original quantity of the order
        /// </summary>
        [JsonProperty("origQty")]
        public decimal OriginalQuantity { get; set; }
        /// <summary>
        /// Executed quantity of the order
        /// </summary>
        [JsonProperty("executedQty")]
        public decimal ExecutedQuantity { get; set; }
        /// <summary>
        /// Cummulative quote quantity of the order
        /// </summary>
        [JsonProperty("cummulativeQuoteQty")]
        public decimal CummulativeQuoteQuantity { get; set; }
        /// <summary>
        /// Status of the order
        /// </summary>
        [JsonConverter(typeof(OrderStatusConverter))]
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Time in force of the order
        /// </summary>
        [JsonConverter(typeof(TimeInForceConverter))]
        public TimeInForce TimeInForce { get; set; }
        /// <summary>
        /// Type of the order
        /// </summary>
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType Type { get; set; }
        /// <summary>
        /// Side of the order
        /// </summary>
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }
    }
}
