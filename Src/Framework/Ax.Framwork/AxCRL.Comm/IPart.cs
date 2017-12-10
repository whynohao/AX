using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm
{
    public interface IPart
    {
    }

    public interface IPartMetadata
    {
        string ProgId { get; }
        PartType PartType { get; }
    }

    public enum PartType
    {
        Bcf = 0,
    }
}
