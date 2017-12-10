using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services.Inspector
{
    public class ParameterInspector : IParameterInspector
    {
        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            //throw new NotImplementedException();
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            return null;
        }
    }

    public class ParameterOperatorBehavior : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // throw new NotImplementedException();
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            // throw new NotImplementedException();
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            // throw new NotImplementedException();
            dispatchOperation.ParameterInspectors.Add(new ParameterInspector());
        }

        public void Validate(OperationDescription operationDescription)
        {
            //throw new NotImplementedException();
        }
    }
}
