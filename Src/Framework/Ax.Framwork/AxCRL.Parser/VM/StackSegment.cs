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
using System.IO;

namespace AxCRL.Parser
{


    class StackSegment<T>
    {

        T[] stack;
	    int _size;
	    int _SP;


	    public StackSegment(int size)
		{

            _SP = -1;
		    _size = size;
		    stack = new T[_size];
		    if(stack==null) 
                Error.OnFatal(0);

		}


        public bool Resize(int size)
	    {
            _size = size;
	        T[] New=new T[size];
	        
            if(New!=null)
		    { 
                Error.OnFatal(0);
		        return false;
		    }

	        for(int i=0; i<_SP; i++)
		        New[i]=stack[i];
	        
	        stack=New;

	        return true;
	    }


        public bool Push(T i)	
        { 
            stack[++_SP] = i;  
            return IsOverflow();
        }

        public T Pop()
        { 
            return stack[_SP--]; 
        }

        public T Pop(int i)	
        {  
            _SP-=i; 
            return stack[_SP]; 
        }

        public T Top()		
        { 
            return stack[_SP]; 
        }

        public T Top(int i)	
        { 
            return stack[_SP - i]; 
        }

        public void Move(int src, int dest, int n)
        {
            for (int i = 0; i < n; i++)
            {
                stack[dest--] = stack[src--];
            }
        }

        public int Ptr()
        { 
            return _SP; 
        }

        public T Replace(T New)
	    {
            T Old=Pop();
		    Push(New);
		    New=Old;
		    return Old;
	    }

        public bool IsEmpty()	
        { 
            return _SP == -1; 
        }

        public bool IsOverflow() 
        { 
            return _SP >= _size; 
        }

        public int Size
        {
            get
            {
                return _size;
            }
        }
        public int SP 
        {
            get
            {
                return _SP;
            }
            set
            {
                _SP = value;
            }
        }
	
        public T this[int i]
	    {	
            get {
		        return stack[i];
            }
            set {
                stack[i]= value;
            }
        }
	
	
        public override String ToString()
        {
            StringWriter o = new StringWriter();
            o.Write("STACK SP={0} ", _SP);
            if (_SP != -1)
            {
                o.Write("MEMORY=");
                for (int i = 0; i < _SP; i++)
                    o.Write("{0},", stack[i]);

                o.Write("{0}", stack[_SP]);
            }
            else
                o.Write("MEMORY=[EMPTY]");
            return o.ToString();
	    }
    }
}
