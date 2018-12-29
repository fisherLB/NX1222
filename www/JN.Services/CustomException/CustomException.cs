using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JN.Services.CustomException
{
    public class CustomException : ApplicationException
    {
        //记录异常的类型
        private CustomExceptionType exceptionType;

        public CustomException(CustomExceptionType type) : base()
        {
            this.exceptionType = type;
        }

        public CustomException(CustomExceptionType type, string message) : base(message)
        {
            this.exceptionType = type;
        }

        public CustomException(string message) : base(message)
        {
            this.exceptionType = CustomExceptionType.InputValidation;
        }

        //序列化
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        //重写message方法,以让它显示相应异常提示信息
        public override string Message
        {
            get
            {
                //根据异常类型从message.xml中读取相应异常提示信息
                return Resource.ResourceProvider.R(base.Message);
                //return string.Format(XmlMessageManager.GetXmlMessage((int)exceptionType), base.Message);
            }
        }
    }

    public enum CustomExceptionType
    {
        InputValidation = 1,
        hint = 2,
        Warning = 3,
        Unknown = 8
    }
}
