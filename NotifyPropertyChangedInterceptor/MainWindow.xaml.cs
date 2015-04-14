namespace NotifyPropertChangedInterceptor
{
    using System.ComponentModel;
    using System.Windows;
    using Interceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.InterceptionExtension;
    using Model;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IUnityContainer _container = new UnityContainer();
        public MainWindow()
        {
            InitializeComponent();

            _container.RegisterType<Foo, Foo>(
                new Interceptor<VirtualMethodInterceptor>(),
                new DefaultInterceptionBehavior<NotifyPropertyChangedBehavior>())
                .AddNewExtension<Interception>();

            var foo = _container.Resolve<Foo>();
            this.DataContext = foo;


            (foo as INotifyPropertyChanged).PropertyChanged += (sender, args) =>
            {
                this.Title = TextBox.Text;
            };
        }
    }
}
