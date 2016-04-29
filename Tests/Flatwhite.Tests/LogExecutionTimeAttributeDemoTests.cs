using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Flatwhite.AutofacIntergration;
using NUnit.Framework;

namespace Flatwhite.Tests
{
    [TestFixture]
    public class LogExecutionTimeAttributeDemoTests
    {
        [SetUp]
        public void SetUp()
        {
            Global.Init();
            Global.Logger = new ConsoleLogger();
        }

        [Test]
        public async Task Test_profiling()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
            builder.RegisterType<CallerLevel0>().AsSelf().EnableInterceptors();
            builder.RegisterType<CallerLevel1>().AsSelf().EnableInterceptors();
            builder.RegisterType<CallerLevel2>().AsSelf().EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<CallerLevel0>();
            var tasks = new List<Task>();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 3; i++)
            {
                tasks.Add(interceptedSvc.CallMethod0());
            }

            await Task.WhenAll(tasks.ToArray());
            sw.Stop();
            Console.WriteLine($"Total : {sw.ElapsedMilliseconds}ms");
        }
    }

    public class CallerLevel0
    {
        private readonly CallerLevel1 _level1;

        public CallerLevel0(CallerLevel1 level1)
        {
            _level1 = level1;
        }

        [LogExecutionTime]
        public virtual async Task CallMethod0()
        {
            await _level1.CallMethod1();
            await Task.Delay(100);
        }
    }

    public class CallerLevel1
    {
        private readonly CallerLevel2 _level2;


        public CallerLevel1(CallerLevel2 level2)
        {
            _level2 = level2;
        }

        [LogExecutionTime]
        public virtual async Task CallMethod1()
        {
            await _level2.CallMethod2();
            await _level2.CallMethod2();
            await Task.Delay(100);
        }
    }

    public class CallerLevel2
    {
        [LogExecutionTime]
        public virtual async Task CallMethod2()
        {
            await Task.Delay(100);
        }
    }
}
