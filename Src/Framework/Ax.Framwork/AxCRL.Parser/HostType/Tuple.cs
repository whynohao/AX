//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.txt file at the root of this distribution. If    //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//


using System;
using System.Collections.Generic;
using System.Text;

namespace AxCRL.Parser
{
    class Tuple<T>
    {
        public Tuple(T item1)
        {
            Item1 = item1;
        }

        public T Item1;
    }

    class Tuple<T, T2> : Tuple<T>
    {
        public Tuple(T item1, T2 item2)
            : base(item1)
        {
            Item2 = item2;
        }

        public T2 Item2;
    }

#if !SILVERLIGHT
    
    class Tuple<T, T2, T3> : Tuple<T, T2>
    {
        public Tuple(T item1, T2 item2, T3 item3)
            : base(item1, item2)
        {
            Item3 = item3;
        }

        public T3 Item3;
    }

    class Tuple<T, T2, T3, T4> : Tuple<T, T2, T3>
    {
        public Tuple(T item1, T2 item2, T3 item3, T4 item4)
            : base(item1, item2, item3)
        {
            Item4 = item4;
        }

        public T4 Item4;
    }

#endif

    static class Tuple
    {
        public static Tuple<T1> Create<T1>(T1 t1)
        {
            return new Tuple<T1>(t1);
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 t1, T2 t2)
        {
            return new Tuple<T1, T2>(t1, t2);
        }

#if !SILVERLIGHT
        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 t1, T2 t2, T3 t3)
        {
            return new Tuple<T1, T2, T3>(t1, t2, t3);
        }

        public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            return new Tuple<T1, T2, T3, T4>(t1, t2, t3, t4);
        }
#endif
    }

}
