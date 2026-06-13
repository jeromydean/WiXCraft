using ExampleInterface.ViewModels;
using ExampleInterface.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleInterface.DependencyInjection
{
  public static class ExampleInterfaceServiceCollectionExtensions
  {
    public static IServiceCollection AddExampleInterfaceUi(this IServiceCollection services)
    {
      services.AddTransient<SetupWizardViewModel>();
      services.AddTransient<SetupWizardView>();
      return services;
    }
  }
}
