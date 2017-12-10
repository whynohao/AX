using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Permission
{
    public sealed class LibPermissionControl
    {
        private static LibPermissionControl _Default = null;
        private static object _LockObj = new object();

        private LibPermissionControl()
        {
        }

        public static LibPermissionControl Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibPermissionControl();
                        }
                    }
                }
                return _Default;
            }
        }

        public bool CanUse(LibHandle handle, string progId)
        {
            bool canUse = false;
            string roleId = handle.RoleId;
            if (handle == LibHandleCache.Default.GetSystemHandle())
            {
                canUse = true;
            }
            else
            {
                LibRolePermission rolePermission = LibRolePermissionCache.Default.GetCacheItem(roleId);
                if (rolePermission.IsUnlimited)
                {
                    canUse = true;
                }
                else
                {
                    if (rolePermission.PermissionDic.ContainsKey(progId))
                    {
                        int dest = (int)FuncPermissionEnum.Use;
                        canUse = (rolePermission.PermissionDic[progId].OperateMark & dest) == dest;
                    }
                }
                if (roleId == string.Empty)
                {
                    canUse = true;
                }
            }
            return canUse;
        }

        public string GetShowCondition(LibHandle handle, string progId, string personId)
        {
            string condition = string.Empty;
            if (handle != LibHandleCache.Default.GetSystemHandle())
            {
                string roleId = handle.RoleId;
                LibRolePermission rolePermission = LibRolePermissionCache.Default.GetCacheItem(roleId);
                if (!rolePermission.IsUnlimited && rolePermission.PermissionDic.ContainsKey(progId))
                {
                    LibPermission permission = rolePermission.PermissionDic[progId];
                    if (!string.IsNullOrEmpty(permission.ShowCondition))
                    {
                        condition = permission.ShowCondition.Trim();
                        condition = condition.Replace("@CURRENT_PERSON", personId);
                    }
                }
            }
            return condition;
        }

        public Dictionary<string, List<LibQueryField>> GetQueryCondition(LibHandle handle, string progId)
        {
            Dictionary<string, List<LibQueryField>> condition = null;
            if (handle != LibHandleCache.Default.GetSystemHandle())
            {
                string roleId = handle.RoleId;
                LibRolePermission rolePermission = LibRolePermissionCache.Default.GetCacheItem(roleId);
                if (!rolePermission.IsUnlimited && rolePermission.PermissionDic.ContainsKey(progId))
                {
                    LibPermission permission = rolePermission.PermissionDic[progId];
                    condition = permission.QueryFieldDic;
                }
            }
            return condition;
        }

        public LibRolePermission GetRolePermission(LibHandle handle, string progId)
        {
            string roleId = handle.RoleId;
            return LibRolePermissionCache.Default.GetCacheItem(roleId);
        }

        public bool HasPermission(LibHandle handle, string progId, FuncPermissionEnum funcPermission)
        {
            bool ret = false;
            if (handle == LibHandleCache.Default.GetSystemHandle())
            {
                ret = true;
            }
            else
            {
                string roleId = handle.RoleId;
                LibRolePermission rolePermission = LibRolePermissionCache.Default.GetCacheItem(roleId);
                if (rolePermission.IsUnlimited)
                {
                    ret = true;
                }
                else if (rolePermission.PermissionDic.ContainsKey(progId))
                {
                    LibPermission permission = rolePermission.PermissionDic[progId];
                    ret = (permission.OperateMark & (int)funcPermission) == (int)funcPermission;
                }
                if (roleId == string.Empty)
                {
                    ret = true;
                }
            }
            return ret;
        }

        public bool HasButtonPermission(LibHandle handle, string progId, string id)
        {
            bool ret = false;
            if (handle == LibHandleCache.Default.GetSystemHandle())
            {
                ret = true;
            }
            else
            {
                string roleId = handle.RoleId;
                LibRolePermission rolePermission = LibRolePermissionCache.Default.GetCacheItem(roleId);
                if (rolePermission.IsUnlimited)
                {
                    ret = true;
                }
                else if (rolePermission.PermissionDic.ContainsKey(progId))
                {
                    LibPermission permission = rolePermission.PermissionDic[progId];
                    ret = !permission.NoUseButton.Contains(id);
                }
                if (roleId == string.Empty)
                {
                    ret = true;
                }
            }
            return ret;
        }
    }
}
