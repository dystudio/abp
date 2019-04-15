using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Autofac;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace Volo.Abp.FluentValidation
{
    public class ApplicationService_FluentValidation_Tests : AbpIntegratedTest<ApplicationService_FluentValidation_Tests.TestModule>
    {
        private readonly IMyAppService _myAppService;

        public ApplicationService_FluentValidation_Tests()
        {
            _myAppService = ServiceProvider.GetRequiredService<IMyAppService>();
        }

        protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
        {
            options.UseAutofac();
        }

        [Fact]
        public void FluentValidation_Test()
        {
            var output = _myAppService.MyMethod(new MyMethodInput
            {
                MyStringValue = "1",
                MyMethodInput2 = new MyMethodInput2()
                {
                    MyStringValue2 = "2"
                },
                MyMethodInput3 = new MyMethodInput3()
                {
                    MyStringValue3 = "3"
                }
            });

            output.ShouldBe("wrongVale");
        }

        [DependsOn(typeof(AbpAutofacModule))]
        [DependsOn(typeof(AbpFluentValidationModule))]
        public class TestModule : AbpModule
        {
            public override void PreConfigureServices(ServiceConfigurationContext context)
            {
                context.Services.OnRegistred(onServiceRegistredContext =>
                {
                    if (typeof(IMyAppService).IsAssignableFrom(onServiceRegistredContext.ImplementationType))
                    {
                        onServiceRegistredContext.Interceptors.TryAdd<FluentValidationInterceptor>();
                    }
                });
            }

            public override void ConfigureServices(ServiceConfigurationContext context)
            {
                context.Services.AddType<MyAppService>();
            }
        }

        public interface IMyAppService
        {
            string MyMethod(MyMethodInput input);
        }

        public class MyAppService : IMyAppService, ITransientDependency
        {
            public string MyMethod(MyMethodInput input)
            {
                return input.MyStringValue;
            }
        }

        public class MyMethodInput : AbstractValidator<MyMethodInput>
        {
            public MyMethodInput()
            {
                RuleFor(x => x.MyStringValue).Equal("MyStringValue");
                RuleFor(x => x.MyMethodInput2.MyStringValue2).Equal("MyStringValue2");
                RuleFor(customer => customer.MyMethodInput3).SetValidator(new MyMethodInput3());
            }

            public string MyStringValue { get; set; }

            public MyMethodInput2 MyMethodInput2 { get; set; }

            public MyMethodInput3 MyMethodInput3 { get; set; }
        }

        public class MyMethodInput2
        {
            public string MyStringValue2 { get; set; }
        }

        public class MyMethodInput3 : AbstractValidator<MyMethodInput3>
        {
            public MyMethodInput3()
            {
                RuleFor(x => x.MyStringValue3).Equal("MyStringValue3");
            }

            public string MyStringValue3 { get; set; }
        }
    }
}