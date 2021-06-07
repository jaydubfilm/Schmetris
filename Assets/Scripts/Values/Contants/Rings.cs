using System.Linq;
using UnityEngine;

namespace StarSalvager.Values
{
    public static class Rings
    {
        //Based on: https://app.diagrams.net/#G1w0byq4RXFKsb0ay8gNeErVM2pPwEPMV6
        private static Ring Ring1 = new Ring(
            new[]
            {
                //[0]
                new Ring.Node {Coordinate = new Vector2Int(0, 0), NodeType = NodeType.Base},
                //[1]
                new Ring.Node {Coordinate = new Vector2Int(1, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(1, -1), NodeType = NodeType.Level},
                //[2]
                new Ring.Node {Coordinate = new Vector2Int(2, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(2, 0), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(2, -1), NodeType = NodeType.Level},
                //[3]
                new Ring.Node {Coordinate = new Vector2Int(3, 1), NodeType = NodeType.Wreck},
                new Ring.Node {Coordinate = new Vector2Int(3, -1), NodeType = NodeType.Wreck},
                //[4]
                new Ring.Node {Coordinate = new Vector2Int(4, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(4, -1), NodeType = NodeType.Level},
                //[5]
                new Ring.Node {Coordinate = new Vector2Int(5, 0), NodeType = NodeType.Wreck},
                //[6]
                new Ring.Node {Coordinate = new Vector2Int(6, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(6, 0), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(6, -1), NodeType = NodeType.Level},
                //[7]
                new Ring.Node {Coordinate = new Vector2Int(7, 0), NodeType = NodeType.Level},
                //[8]
                new Ring.Node {Coordinate = new Vector2Int(8, 1), NodeType =  NodeType.Wreck},
                new Ring.Node {Coordinate = new Vector2Int(8, 0), NodeType =  NodeType.Wreck},
                new Ring.Node {Coordinate = new Vector2Int(8, -1), NodeType = NodeType.Wreck},
                //[9]
                new Ring.Node {Coordinate = new Vector2Int(9, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(9, -1), NodeType = NodeType.Level},
                //[10]
                new Ring.Node {Coordinate = new Vector2Int(10, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(10, -1), NodeType = NodeType.Level},
                //[11]
                new Ring.Node {Coordinate = new Vector2Int(11, 1), NodeType =  NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(11, 0), NodeType =  NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(11, -1), NodeType = NodeType.Level},
                //[12]
                new Ring.Node {Coordinate = new Vector2Int(12, 1), NodeType =  NodeType.Wreck},
                new Ring.Node {Coordinate = new Vector2Int(12, -1), NodeType = NodeType.Wreck},
                //[13]
                new Ring.Node {Coordinate = new Vector2Int(13, 0), NodeType = NodeType.Level},
            },
            new[]
            {
                //[0]
                new Vector2Int(0,1),
                new Vector2Int(0,2),
                //[1]
                new Vector2Int(1,3),
                new Vector2Int(1,4),
                new Vector2Int(2,4),
                new Vector2Int(2,5),
                //[2]
                new Vector2Int(3,6),
                new Vector2Int(4,6),
                new Vector2Int(5,6),
                new Vector2Int(5, 7),
                //[3]
                new Vector2Int(6,8),
                new Vector2Int(6,9),
                new Vector2Int(7,9),
                //[4]
                new Vector2Int(8,10),
                new Vector2Int(9,10),
                //[5]
                new Vector2Int(10,11),
                new Vector2Int(10,12),
                new Vector2Int(10,13),
                //[6]
                new Vector2Int(11, 14),
                new Vector2Int(12, 14),
                new Vector2Int(13, 14),
                //[7]
                new Vector2Int(14,15),
                new Vector2Int(14,16),
                new Vector2Int(14,17),
                //[8]
                new Vector2Int(15,18),
                new Vector2Int(16,18),
                new Vector2Int(16,19),
                new Vector2Int(17,19),
                //[9]
                new Vector2Int(18,20),
                new Vector2Int(18,21),
                new Vector2Int(19,21),
                //[10]
                new Vector2Int(20,22),
                new Vector2Int(21,22),
                new Vector2Int(21,23),
                new Vector2Int(21,24),
                //[11]
                new Vector2Int(22,25),
                new Vector2Int(23,25),
                new Vector2Int(23,26),
                new Vector2Int(24,26),
                //[11]
                new Vector2Int(25,27),
                new Vector2Int(26,27),
            });

        private static Ring Ring2 = new Ring(
            new[]
            {
                //[0]
                new Ring.Node {Coordinate = new Vector2Int(0, 0), NodeType = NodeType.Base},
                //[1]
                new Ring.Node {Coordinate = new Vector2Int(1, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(1, 0), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(1, -1), NodeType = NodeType.Level},
                //[2]
                new Ring.Node {Coordinate = new Vector2Int(2, 1), NodeType = NodeType.Wreck},
                new Ring.Node {Coordinate = new Vector2Int(2, -1), NodeType = NodeType.Wreck},
                //[3]
                new Ring.Node {Coordinate = new Vector2Int(3, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(3, -1), NodeType = NodeType.Level},
                //[4]
                new Ring.Node {Coordinate = new Vector2Int(4, 0), NodeType = NodeType.Level}, 
                //[5]
                new Ring.Node {Coordinate = new Vector2Int(5, 1), NodeType = NodeType.Wreck},
                new Ring.Node {Coordinate = new Vector2Int(5, 0), NodeType = NodeType.Wreck},
                new Ring.Node {Coordinate = new Vector2Int(5, -1), NodeType = NodeType.Wreck},
                //[6]
                new Ring.Node {Coordinate = new Vector2Int(6, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(6, -1), NodeType = NodeType.Level},
                //[7]
                new Ring.Node {Coordinate = new Vector2Int(7, 0), NodeType = NodeType.Level}, 
                //[8]
                new Ring.Node {Coordinate = new Vector2Int(8, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(8, 0), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(8, -1), NodeType = NodeType.Level},
                //[9]
                new Ring.Node {Coordinate = new Vector2Int(9, 1), NodeType = NodeType.Wreck},
                new Ring.Node {Coordinate = new Vector2Int(9, -1), NodeType = NodeType.Wreck},
                //[10]
                new Ring.Node {Coordinate = new Vector2Int(10, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(10, 0), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(10, -1), NodeType = NodeType.Level},
                //[11]
                new Ring.Node {Coordinate = new Vector2Int(11, 1), NodeType = NodeType.Level},
                new Ring.Node {Coordinate = new Vector2Int(11, -1), NodeType = NodeType.Level},
                //[12]
                new Ring.Node {Coordinate = new Vector2Int(12, 0), NodeType = NodeType.Wreck}, 
                //[13]
                new Ring.Node {Coordinate = new Vector2Int(13, 0), NodeType = NodeType.Level}, 
                
            }, 
            new[]
            {
                //[0]
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(0, 3),
                //[1]
                new Vector2Int(1, 4),
                new Vector2Int(2, 4),
                new Vector2Int(2, 5),
                new Vector2Int(3, 5),
                //[2]
                new Vector2Int(4, 6),
                new Vector2Int(4, 7),
                new Vector2Int(5, 6),
                new Vector2Int(5, 7),
                //[3]
                new Vector2Int(6, 8),
                new Vector2Int(7, 8),
                //[4]
                new Vector2Int(8, 9),
                new Vector2Int(8, 10),
                new Vector2Int(8, 11),
                //[5]
                new Vector2Int(9, 12),
                new Vector2Int(10, 12),
                new Vector2Int(11, 13),
                //[6]
                new Vector2Int(12, 14),
                new Vector2Int(13, 14),
                //[7]
                new Vector2Int(14, 15),
                new Vector2Int(14, 16),
                new Vector2Int(14, 17),
                //[8]
                new Vector2Int(15, 18),
                new Vector2Int(16, 19),
                new Vector2Int(17, 19),
                //[9]
                new Vector2Int(18, 20),
                new Vector2Int(18, 21),
                new Vector2Int(19, 21),
                new Vector2Int(19, 22),
                //[10]
                new Vector2Int(20, 23),
                new Vector2Int(21, 23),
                new Vector2Int(22, 24),
                //[11]
                new Vector2Int(23, 25),
                new Vector2Int(24, 25),
                //[12]
                new Vector2Int(25, 26),
            });
        
        public static Ring[] RingMaps = new[]
        {
            Ring1,
            Ring2
        };
    }

    public class Ring
    {
        public struct Node
        {
            public Vector2Int Coordinate;
            public NodeType NodeType;
        }
        
        public readonly Node[] Nodes;
        public readonly Vector2Int[] Connections;

        public Ring(in Node[] nodes, in Vector2Int[] connections)
        {
            Nodes = nodes;
            Connections = connections;
        }

        public Vector2Int GetCoordinateFromIndex(in int index) => Nodes[index].Coordinate;

        public int GetIndexFromCoordinate(in Vector2Int coordinate)
        {
            var temp = coordinate;

            return Nodes.ToList().FindIndex(x => x.Coordinate == temp);
        }
    }
}
