using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using hhnl.ProcessIsolation.Communication;
using Newtonsoft.Json;
using NUnit.Framework;

namespace hhnl.ProcessIsolation.Tests
{
    public class IsolatedProcessChannelTests
    {
        private IsolatedProcessChannel<string> _clientChannel;
        private IsolatedProcessChannel<string> _serverChannel;

        [SetUp]
        public async Task Setup()
        {
            var transportSerializer = new MockSerializer();
            var pipeName = Guid.NewGuid().ToString();

            var pipeServer = new IsolatedProcessServerStream(new NamedPipeServerStream(pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous));

            var pipeClient =
                new IsolatedProcessClientStream(new NamedPipeClientStream(".",
                    pipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous));

            _clientChannel = new IsolatedProcessChannel<string>(pipeClient, transportSerializer);
            _serverChannel = new IsolatedProcessChannel<string>(pipeServer, transportSerializer);

            var serverStart = _serverChannel.StartAsync();

            await _clientChannel.StartAsync();
            await serverStart;
        }

        [TearDown]
        public async Task TearDown()
        {
            await Task.WhenAll(_clientChannel.StopAsync(), _serverChannel.StopAsync());
            await _clientChannel.DisposeAsync();
            await _serverChannel.DisposeAsync();
        }
        
        [Test]
        public async Task Client_can_send_to_server()
        {
            // Act
            await _serverChannel.EnqueueMessageAsync("test");
            var result = await _clientChannel.ReceiveAsync();

            // Assert
            Assert.AreEqual("test", result, "Message length mismatch.");
        }

        [Test]
        public async Task Server_can_send_to_client()
        {
            // Act
            await _clientChannel.EnqueueMessageAsync("test");
            var result = await _serverChannel.ReceiveAsync();

            // Assert
            Assert.AreEqual("test", result, "Message length mismatch.");
        }

        [Test]
        public async Task Can_send_bi_directional()
        {
            const int messageCount = 1000;
            
            var clientSendTask = Task.Run(async () =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    await _clientChannel.EnqueueMessageAsync(i.ToString());
                }
            });
            var clientReceiveTask = Task.Run(async () =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    await _clientChannel.ReceiveAsync();
                }
            });
            var serverSendTask = Task.Run(async () =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    await _serverChannel.EnqueueMessageAsync(i.ToString());
                }
            });
            var serverReceiveTask = Task.Run(async () =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    await _serverChannel.ReceiveAsync();
                }
            });

            await Task.WhenAll(clientSendTask, clientReceiveTask, serverSendTask, serverReceiveTask);
        }

        [Test]
        public async Task Can_send_simultaneously_directional()
        {
            const int messageCount = 1000;
            
            var clientSendTask1 = Task.Run(async () =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    await _clientChannel.EnqueueMessageAsync(i.ToString());
                }
            });
            var clientSendTask2 = Task.Run(async () =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    await _clientChannel.EnqueueMessageAsync(i.ToString());
                }
            });
            var clientSendTask3 = Task.Run(async () =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    await _clientChannel.EnqueueMessageAsync(i.ToString());
                }
            });
            var clientSendTask4 = Task.Run(async () =>
            {
                for (var i = 0; i < messageCount; i++)
                {
                    await _clientChannel.EnqueueMessageAsync(i.ToString());
                }
            });
            
            for (var i = 0; i < messageCount * 4; i++)
            {
                await _serverChannel.ReceiveAsync();
            }

            await Task.WhenAll(clientSendTask1, clientSendTask2, clientSendTask3, clientSendTask4);
        }
        
        private class MockSerializer : ITransportSerializer
        {
            public (int Length, byte[] buffer) Serialize<T>(T message)
            {
                var json = JsonConvert.SerializeObject(message);
                var bytes = Encoding.Unicode.GetBytes(json);
                return (bytes.Length, bytes);
            }

            public T Deserialize<T>(ReadOnlySpan<byte> bytes)
            {
                var json = Encoding.Unicode.GetString(bytes);
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}