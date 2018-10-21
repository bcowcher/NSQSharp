About This Fork
===============

Essentially I needed a .Net Core ready library for NSQ and since I was using this for my current work I decided to take a stab at making this one ready for it.

I've left the windows specific parts in the NsqSharp directory and migrated everything else pretty much as is into the NsqCore directory with a few minor changes (mostly around the reflection parts and visibility of certain objects) as the NsqSharp project now depends on NsqCore.

Technically, it *should* just be a drop in replacement in any .Net Core app but I can't give a 100% guarantee on that.

NsqSharp
========

[![Build status](https://ci.appveyor.com/api/projects/status/s4yca701nhj1db9t/branch/master?svg=true)](https://ci.appveyor.com/project/judwhite/nsqsharp/branch/master)&nbsp;&nbsp;[![License](http://img.shields.io/:license-mit-blue.svg)](http://doge.mit-license.org)&nbsp;&nbsp;[![NuGet version](https://badge.fury.io/nu/nsqsharp.svg)](https://www.nuget.org/packages/nsqsharp)

A .NET client library for [NSQ](https://github.com/nsqio/nsq), a realtime distributed messaging platform.

Check out this [slide deck](https://speakerdeck.com/snakes/nsq-nyc-golang-meetup?slide=19) for a quick intro to NSQ.

Watch [Spray Some NSQ On It](https://www.youtube.com/watch?v=CL_SUzXIUuI) by co-author [Matt Reiferson](https://github.com/mreiferson) for an under 30-minute intro to NSQ as a messaging platform.

## Project Status

- Used in Production.
- Maintained. Issues and Pull Requests will be responded to.

## Quick Install

NsqSharp is a client library that talks to the `nsqd` (message queue) and `nsqlookupd` (topic discovery service). See the slides above for more information about their roles.

Download [`nsqd.exe`](https://github.com/judwhite/NsqSharp/blob/master/nsq-0.3.2-bin/nsqd.exe?raw=true) and [`nsqlookupd.exe`](https://github.com/judwhite/NsqSharp/blob/master/nsq-0.3.2-bin/nsqlookupd.exe?raw=true) and run them from the command line:

```
nsqlookupd

nsqd -lookupd-tcp-address=127.0.0.1:4160
```

Or, to install as Windows Services:

```
mkdir c:\nsq\data

copy /y nsqd.exe c:\nsq
copy /y nsqlookupd.exe c:\nsq

sc create nsqlookupd binpath= "c:\nsq\nsqlookupd.exe" start= auto DisplayName= "nsqlookupd"
sc description nsqlookupd "nsqlookupd 0.3.2"
sc start nsqlookupd

sc create nsqd binpath= "c:\nsq\nsqd.exe -mem-queue-size=0 -lookupd-tcp-address=127.0.0.1:4160 -data-path=c:\nsq\data" start= auto DisplayName= "nsqd"
sc description nsqd "nsqd 0.3.2"
sc start nsqd
```

You can also build these files from source: https://github.com/nsqio/nsq (official), or https://github.com/judwhite/nsq (fork) to add the ability to run as a Windows Service.

## C# Examples

`PM> Install-Package NsqSharp`

More examples are in the [Examples](https://github.com/judwhite/NsqSharp/tree/master/Examples) folder.

#### Simple Producer

```cs
using System;
using NsqSharp;

class Program
{
    static void Main()  
    {
        var producer = new Producer("127.0.0.1:4150");
        producer.Publish("test-topic-name", "Hello!");

        Console.WriteLine("Enter your message (blank line to quit):");
        string line = Console.ReadLine();
        while (!string.IsNullOrEmpty(line))
        {
            producer.Publish("test-topic-name", line);
            line = Console.ReadLine();
        }

        producer.Stop();
    }
}
```

#### Simple Consumer

```cs
using System;
using System.Text;
using NsqSharp;

class Program
{
    static void Main()  
    {
        // Create a new Consumer for each topic/channel
        var consumer = new Consumer("test-topic-name", "channel-name");
        consumer.AddHandler(new MessageHandler());
        consumer.ConnectToNsqLookupd("127.0.0.1:4161");

        Console.WriteLine("Listening for messages. If this is the first execution, it " +
                          "could take up to 60s for topic producers to be discovered.");
        Console.WriteLine("Press enter to stop...");
        Console.ReadLine();

        consumer.Stop();
    }
}

public class MessageHandler : IHandler
{
    /// <summary>Handles a message.</summary>
    public void HandleMessage(IMessage message)
    {
        string msg = Encoding.UTF8.GetString(message.Body);
        Console.WriteLine(msg);
    }

    /// <summary>
    /// Called when a message has exceeded the specified <see cref="Config.MaxAttempts"/>.
    /// </summary>
    /// <param name="message">The failed message.</param>
    public void LogFailedMessage(IMessage message)
    {
        // Log failed messages
    }
}
```

## NsqSharp.Bus

The classes in the `NsqSharp.Bus` namespace provide conveniences for large scale applications:
- Interoperating with dependency injection containers.
- Separation of concerns with regards to message routing, serialization, and error handling.
- Abstracting the details of `Producer` and `Consumer` from message sending and handling.

The [PingPong](https://github.com/judwhite/NsqSharp/tree/master/Examples/PingPong) and [PointOfSale](https://github.com/judwhite/NsqSharp/tree/master/Examples/PointOfSale) examples highlight using:

- [StructureMap](https://github.com/structuremap/structuremap) (any dependency injection container will work)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) (any serialization method can be chosen)
- [NLog](https://github.com/NLog/NLog) (any logger can be used)
- SQL Server for auditing (See [MessageAuditor.cs](https://github.com/judwhite/NsqSharp/blob/master/Examples/PointOfSale/PointOfSale.Common/Nsq/MessageAuditor.cs) and [TransportAuditHandler.cs](https://github.com/judwhite/NsqSharp/blob/master/Examples/PointOfSale/PointOfSale.Handlers.Audit/Handlers/TransportAuditHandler.cs) in the [PointOfSale](https://github.com/judwhite/NsqSharp/tree/master/Examples/PointOfSale) example)

Applications initiated with [`BusService.Start`](https://github.com/judwhite/NsqSharp/blob/master/Examples/PingPong/Program.cs#L24) can be installed as a Windows Service using `sc create`. When in console mode the application will gracefully shutdown with `Ctrl+C`. When running as a Windows Service stopping the service or rebooting/shutting down the machine will do a graceful shutdown.

NsqSharp has no external dependencies. StructureMap, Autofac, and Newtonsoft.Json are supported through convenience classes which use reflection for the initial wire-up. Other containers and serializers can be used by implementing [`IObjectBuilder`](https://github.com/judwhite/NsqSharp/blob/master/NsqSharp/Bus/Configuration/IObjectBuilder.cs) and [`IMessageSerializer`](https://github.com/judwhite/NsqSharp/blob/master/NsqSharp/Bus/Configuration/IMessageSerializer.cs) wrappers in your code.

## NsqSharp Project Goals
- Structurally similar to the official [go-nsq](https://github.com/nsqio/go-nsq) client.
- Up to date with the latest stable release of go-nsq.
- Provide similar behavior and semantics as the official package.
- Unobtrusive. No external dependencies. Publishing message contracts does not require a reference to NsqSharp.

## Pull Requests

Pull requests and issues are very welcome and appreciated.

When submitting a pull request please keep in mind we're trying to stay as close to [go-nsq](https://github.com/nsqio/go-nsq) as possible. This sometimes means writing C# which looks more like Go and follows their file layout. Code in the `NsqSharp.Bus` namespace should follow C# conventions and more or less look like other code in this namespace.

## License

This project is open source and released under the [MIT license.](LICENSE)
