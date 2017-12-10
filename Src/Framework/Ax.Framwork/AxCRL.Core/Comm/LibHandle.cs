using AxCRL.Comm.Runtime;
using AxCRL.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Comm
{
    /// <summary>
    /// 用户句柄
    /// </summary>
    public class LibHandle
    {
        private string _Handle;

        public string Handle
        {
            get { return _Handle; }
            set { _Handle = value; }
        }
        private LibHandeleType _Type;

        public LibHandeleType Type
        {
            get { return _Type; }
            set {  _Type= value; }
        }
        private string _UserId;

        public string UserId
        {
            get { return _UserId; }
            set { _UserId = value; }
        }
        private string _PersonId;

        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }
        private string _PersonName;

        public string PersonName
        {
            get { return _PersonName; }
            set { _PersonName = value; }
        }
        private DateTime _CreateTime;

        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

        private string _RoleId;

        public string RoleId
        {
            get { return _RoleId; }
            set { _RoleId = value; }
        }

        private string _LoginIp;
        public string LogIp
        {
            get { return _LoginIp; }
            set { _LoginIp = value; }
        }
        private string _Token=Guid.NewGuid().ToString();
        /// <summary>
        /// 登录有效性令牌信息
        /// </summary>
        public string Token
        {
            get { return _Token; }
            set { _Token = value; }
        }
        private DateTime _TokenValidTime = DateTime.Now.AddMinutes(EnvProvider.Default.TokenValidMinutes);
        /// <summary>
        /// 登录令牌的有效截止时间
        /// </summary>
        public DateTime TokenValidTime
        {
            get { return _TokenValidTime; }
            set { _TokenValidTime = value; }
        }        
        public LibHandle()
        {

        }
        /// <summary>
        /// 获取用来验证的Token。会将有效期延长指定时间，避免在校验前被改变
        /// </summary>
        /// <returns></returns>
        public string GetToCheckToken()
        {
            this._TokenValidTime = DateTime.Now.AddMinutes(EnvProvider.Default.TokenValidMinutes);
            LibHandleCache.Default.Set(this.Handle, this);//更改信息后重新放入缓存
            return _Token;
        }
        /// <summary>
        /// 生成新的Token。
        /// 由外部将其重新放入Redis缓存中
        /// </summary>
        public void SetNewToken()
        {
            this._TokenValidTime = DateTime.Now.AddMinutes(EnvProvider.Default.TokenValidMinutes);
            this._Token = Guid.NewGuid().ToString();
        }
        public LibHandle(LibHandeleType type, string userId, string personId, string personName, string roleId,string loginIp)
        {
            _Handle = Guid.NewGuid().ToString();
            this._Type = type;
            this._UserId = userId;
            this._PersonId = personId;
            this._PersonName = personName;
            this._CreateTime = DateTime.Now;
            this._RoleId = roleId;
            this._LoginIp = loginIp;
        }
        public LibHandle(LibHandeleType type, string userId, string roleId)
        {
            _Handle = Guid.NewGuid().ToString();
            this._Type = type;
            this._UserId = userId;
            this._CreateTime = DateTime.Now;
            this._RoleId = roleId;
        }

        public LibHandle(LibHandeleType type, string userId, string personId, string personName, string roleId)
        {
            _Handle = Guid.NewGuid().ToString();
            this._Type = type;
            this._UserId = userId;
            this._PersonId = personId;
            this._PersonName = personName;
            this._CreateTime = DateTime.Now;
            this._RoleId = roleId;
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }
        public static bool operator == (LibHandle obj1, LibHandle obj2)
        {
            return Object.Equals(obj1, obj2);
        }
        public static bool operator !=(LibHandle obj1, LibHandle obj2)
        {
            return !Object.Equals(obj1, obj2);
        }
        public static bool Equals(LibHandle obj1, LibHandle obj2)
        {
                if (obj1.Handle == obj2.Handle)
                    return true;
                return false;
           
        }
        
        public override bool Equals(object obj)
        {
            LibHandle lb = obj as LibHandle;
            if (lb == null)
                return false;
            if (this.Handle == lb.Handle )
                return true;
            return false;
        }
      
  

    }
    /// <summary>
    /// 用户句柄类型
    /// </summary>
    public enum LibHandeleType
    {
        Unknown = 0,
        PC = 1,
        Phone = 2,
        Pad = 3,
        /// <summary>
        /// 跨站点访问的临时登录
        /// </summary>
        CrossSiteCall = 4,
    }
}
