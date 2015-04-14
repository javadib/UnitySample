namespace NotifyPropertChangedInterceptor.Interceptions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using Microsoft.Practices.Unity.InterceptionExtension;

    class NotifyPropertyChangedBehavior : IInterceptionBehavior
    {
        private event PropertyChangedEventHandler PropertyChanged;

        private readonly MethodInfo _addEventMethodInfo =
            typeof(INotifyPropertyChanged).GetEvent("PropertyChanged").GetAddMethod();

        private readonly MethodInfo _removeEventMethodInfo =
            typeof(INotifyPropertyChanged).GetEvent("PropertyChanged").GetRemoveMethod();

        
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            if (input.MethodBase == _addEventMethodInfo)
            {
                return AddEventSubscription(input);
            }

            if (input.MethodBase == _removeEventMethodInfo)
            {
                return RemoveEventSubscription(input);
            }
            
            if (IsPropertySetter(input))
            {
                return InterceptPropertySet(input, getNext);
            }
            
            return getNext()(input, getNext);
        }

        public bool WillExecute
        {
            get { return true; }
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            yield return typeof (INotifyPropertyChanged);
        }

        private IMethodReturn AddEventSubscription(IMethodInvocation input)
        {
            var subscriber = (PropertyChangedEventHandler)input.Arguments[0];
            PropertyChanged += subscriber;

            return input.CreateMethodReturn(null);
        }

        private IMethodReturn RemoveEventSubscription(IMethodInvocation input)
        {
            var subscriber = (PropertyChangedEventHandler)input.Arguments[0];
            PropertyChanged -= subscriber;

            return input.CreateMethodReturn(null);
        }

        private bool IsPropertySetter(IMethodInvocation input)
        {
            return input.MethodBase.IsSpecialName && input.MethodBase.Name.StartsWith("set_");
        }

        private IMethodReturn InterceptPropertySet(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var propertyName = input.MethodBase.Name.Substring(4);

            var subscribers = PropertyChanged;
            if (subscribers != null)
            {
                subscribers(input.Target, new PropertyChangedEventArgs(propertyName));
            }

            return getNext()(input, getNext);
        }
    }
}