using System.Collections.Generic;

namespace XSAppModel.XStudio
{

    public class LineParamNode
    {
        public LineParamNode() : this(0, 0)
        {
        }

        public LineParamNode(int pos, int value)
        {
            Pos = pos;
            Value = value;
        }

        /* Properties */
        public int Pos;
        public int Value;
    }

    public class LineParam
    {
        public LineParam()
        {
            nodeLinkedList = new LinkedList<LineParamNode>();
        }

        /* This class is derived from System.Runtime.Serialization.ISerializable
         * Using custom functions to pack or unpack data
         */

        public LinkedList<LineParamNode> nodeLinkedList;

        /* LineParam property of a singing track cannot be null or empty,
        * it should at least contain the following two magic point.
        */
        public void setDefault()
        {
            nodeLinkedList = new LinkedList<LineParamNode>();
            nodeLinkedList.AddLast(new LineParamNode(-192000, 0));
            nodeLinkedList.AddLast(new LineParamNode(1073741823, 0));
        }
    }
}