using System;
using System.Collections.Generic;

using java.util;

namespace IKVM.Maven.Sdk.Tasks.Extensions
{

    class TreeSetWrapper<T> : SetWrapper<T>, ISet<T>
    {

        readonly TreeSet set;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="set"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TreeSetWrapper(TreeSet set) :
            base(set)
        {
            this.set = set ?? throw new ArgumentNullException(nameof(set));
        }

    }

}