using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Assets.Scripts.Utility;
using UnityEngine;
using Random = System.Random;

namespace MapMagic
{
    [Serializable]
    [GeneratorMenu(menu = "CustomMap", name = "Valley", disengageable = true)]
    public class ValleyGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Objects);
        public Output output = new Output("Output", InoutType.Map);

        public bool AbandonSingleNodes = false;
        public int MaxConnectionCount = 3;
        public float ConnectChance = 0.75f;
        public int LineSize = 10;
        public float ConnectDistance = 1;

        public class Node
        {
            public Node(Coord origin)
            {
                Origin = origin;
            }

            public List<Node> Links = new List<Node>();
            public readonly Coord Origin;
        }

        public override IEnumerable<Input> Inputs()
        {
            yield return input;
        }

        public override IEnumerable<Output> Outputs()
        {
            yield return output;
        }

        public override void Generate(Chunk chunk, Biome currentBiome = null)
        {
            //getting input
            var dst = chunk.defaultMatrix;

            var points = input.GetObject(chunk) as SpatialHash;
            if (points == null)
            {
                output.SetObject(chunk, dst);
                return;
            }

            //return on stop/disable/null input
            if (chunk.stop || dst == null) return;
            if (!enabled)
            {
                output.SetObject(chunk, dst);
                return;
            }

            var random = new Random(MapMagic.instance.seed + seed + chunk.coord.GetHashCode());
            
            Dictionary<Coord, Node> links = new Dictionary<Coord, Node>();

            foreach (var first in points.AllObjs())
            {
                var firstCoord = new Coord(Mathf.RoundToInt(first.pos.x), Mathf.RoundToInt(first.pos.y));
                if (!AbandonSingleNodes)
                {
                    dst.DrawCircle(firstCoord, LineSize);
                }

                Node firstNode;
                if (!links.TryGetValue(firstCoord, out firstNode))
                {
                    firstNode = new Node(firstCoord);
                    links[firstCoord] = firstNode;
                }
                
                foreach (var second in points.AllObjs())
                {
                    if (firstNode.Links.Count > MaxConnectionCount)
                    {
                        break;
                    }

                    var secondCoord = new Coord(Mathf.RoundToInt(second.pos.x), Mathf.RoundToInt(second.pos.y));
                    if (firstCoord == secondCoord)
                    {
                        continue;
                    }

                    var dist = Coord.Distance(firstCoord, secondCoord);
                    if (!(dist < ConnectDistance))
                    {
                        continue;
                    }
                    
                    var connectRoll = (float)random.NextDouble();
                    if (connectRoll > ConnectChance)
                    {
                        continue;
                    }

                    Node secondNode;
                    if (!links.TryGetValue(secondCoord, out secondNode))
                    {
                        secondNode = new Node(secondCoord);
                        links[secondCoord] = secondNode;
                    }

                    if (secondNode.Links.Count > MaxConnectionCount)
                    {
                        continue;
                    }

                    if (AlreadyLinked(firstNode, secondNode) || AlreadyLinked(secondNode, firstNode))
                    {
                        continue;
                    }

                    firstNode.Links.Add(secondNode);
                    secondNode.Links.Add(firstNode);
                    
                    dst.DrawLine(firstCoord, secondCoord, LineSize, (x, z, width, start, end) =>
                    {
                        dst.DrawCircle(new Coord(x, z), width);
                        return true;
                    });
                }
            }

            //mask and safe borders
            if (chunk.stop) return;

            //setting output
            if (chunk.stop) return;
            output.SetObject(chunk, dst);
        }

        private bool AlreadyLinked(Node rootNode, Node checkingNode, HashSet<Node> alreadyChecked = null)
        {
            // If the root isn't connected to anything, it definitely isn't connected to this node
            if (rootNode.Links.Count == 0)
            {
                return false;
            }

            if (alreadyChecked == null)
            {
                alreadyChecked = new HashSet<Node>();
            }

            if (checkingNode.Links.Contains(rootNode))
            {
                // The checking node contains the root - they are already linked
                return true;
            }

            // To prevent accidentally travelling backwards
            alreadyChecked.Add(checkingNode);

            // Check all of the checkingnodes nodes recursively
            for (int i = 0; i < checkingNode.Links.Count; i++)
            {
                var link = checkingNode.Links[i];

                if (alreadyChecked.Contains(link))
                {
                    // Don't travel backwards
                    continue;
                }

                var result = AlreadyLinked(rootNode, link, alreadyChecked);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public override void OnGUI()
        {
            //inouts
            layout.Par(20);
            input.DrawIcon(layout);
            output.DrawIcon(layout);

            layout.Field(ref seed, "Seed");
            layout.Field(ref LineSize, "Line Size");
            layout.Field(ref ConnectDistance, "Connect Distance");
            layout.Field(ref ConnectChance, "Connect Chance");
            layout.Field(ref MaxConnectionCount, "Max Conn.");
            layout.Field(ref AbandonSingleNodes, "Abandon Singles");
        }
    }
}