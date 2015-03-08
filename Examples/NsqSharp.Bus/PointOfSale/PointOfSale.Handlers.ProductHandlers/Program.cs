﻿using System;
using NsqSharp.Bus;
using NsqSharp.Bus.Configuration;
using PointOfSale.Common;
using PointOfSale.Messages.Products.Commands;

namespace PointOfSale.Handlers.ProductHandlers
{
    class Program
    {
        static void Main()
        {
            PointOfSaleBus.Start(new ChannelProvider(), new BusStateChangedHandler());
        }

        public class BusStateChangedHandler : IBusStateChangedHandler
        {
            public void OnBusStarting(IBusConfiguration config) { }
            public void OnBusStopping(IBusConfiguration config, IBus bus) { }
            public void OnBusStopped(IBusConfiguration config) { }

            public void OnBusStarted(IBusConfiguration config, IBus bus)
            {
                if (bus == null)
                    throw new ArgumentNullException("bus");

                bus.Send<GetProductsCommand>();
            }
        }
    }
}
