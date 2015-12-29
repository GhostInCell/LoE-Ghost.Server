using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PNet
{
    public interface IRpcFilter<TInfo>
    {
        /// <summary>
        /// filter an rpc with the specified information
        /// </summary>
        /// <param name="info"></param>
        /// <returns>false, if you want to prevent the rpc from processing</returns>
        bool Filter(TInfo info);
    }

    public interface IComponentRpcFilterProvider<TInfo>
    {
        IRpcFilter<TInfo> GetFilter(IComponentInfoRpcProvider<TInfo> provider);
    }
}
