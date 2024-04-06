using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AStarCtrl
{
    private static Vector2Int[] neiOffset = {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int( 0, 1), new Vector2Int(0, -1),
    };

    public static bool CanMovePos(Vector2Int basePos,Vector2Int targetPos, List<IAStarGround> padsList)
    {
        var dic = padsList.ToDictionary(pad => pad.GetPos(), pad => pad);
        float distance = Vector2Int.Distance(basePos, targetPos);

        Vector2Int lastCheckPos = new Vector2Int(-99999, -99999);
        for (float t = 0; t <= 1; t += 0.1f / distance) {
            Vector2Int checkPos = new Vector2Int(
                Mathf.RoundToInt(Mathf.Lerp((float) basePos.x, (float) targetPos.x,t)), 
                Mathf.RoundToInt(Mathf.Lerp((float) basePos.y, (float) targetPos.y,t)));

            if (lastCheckPos != checkPos) {
                lastCheckPos = checkPos;
                if (!dic.ContainsKey(checkPos) || !dic[checkPos].CanCross()) {
                    return false;
                }
            }
        }
        return true;
    }

    public static bool AStar(Vector2Int startPos, Vector2Int targetPos, List<IAStarGround> PadsList, out Stack<IAStarGround> path) {
        path = new Stack<IAStarGround>();

        if (startPos == targetPos) return false;

        Dictionary<Vector2Int, Node> allList = new Dictionary<Vector2Int, Node>();
        foreach (var pad in PadsList)
        {
            var pos = pad.GetPos();
            allList.Add(pos, new Node(pad, targetPos, pad.CanCross()));
        }

        Stack<IAStarGround> OptimizePath(Node node)
        {
            var nowPath = GetPath(node);
            List<IAStarGround> pathList = new List<IAStarGround>(nowPath);
            pathList.Reverse();
            pathList.Add(allList[startPos].Pos);
            int i = 0;
            while (i < pathList.Count - 2) {
                bool foundShortcut = false;
                for (int j = pathList.Count - 1; j > i + 1; j--) {
                    if (CanMovePos(pathList[i].GetPos(), pathList[j].GetPos(), PadsList)) {
                        pathList.RemoveRange(i + 1, j - i - 1);
                        foundShortcut = true;
                        break;
                    }
                }

                if (!foundShortcut) {
                    i++;
                }
            }
            return new Stack<IAStarGround>(pathList);
        }

        List<Node> openList = new List<Node>();
        List<Node> closeList = new List<Node>();

        Node startNode;
        if (!allList.TryGetValue(startPos, out startNode)) {
            Debug.LogError("AStarFall:NullStartPos");
            return false;
        }

        Node closeN = startNode;
        openList.Add(closeN);

        while (openList.Count > 0) {
            Node currntN = openList[0];

            for (int i = 0; i < openList.Count; i++) {
                Node node = openList[i];
                if (node.C < closeN.C) closeN = node;
                if (node.PC < currntN.PC || node.PC == currntN.PC && node.C < currntN.C) currntN = node;
            }

            openList.Remove(currntN);
            closeList.Add(currntN);

            if (currntN.Pos.GetPos() == targetPos) {
                path = OptimizePath(currntN);
                return true;
            }

            foreach (var nei in neiOffset) {
                Vector2Int neighorPos = currntN.Pos.GetPos() + nei;
                if (!allList.TryGetValue(neighorPos, out var neighorN) || 
                    closeList.Contains(neighorN) || 
                    neighorN.Pos.GetPos()!= targetPos && !neighorN.CanMove) continue;
                int p = currntN.P + GetPathDis(currntN.Pos, neighorN.Pos.GetPos());
                bool notInList = !openList.Contains(neighorN);
                if (notInList || p < neighorN.P) {
                    neighorN.P = p;
                    neighorN.Parent = currntN;

                    if (notInList) openList.Add(neighorN);
                }
            }
        }

        path = OptimizePath(closeN);
        return false;
    }

    class Node {
        public IAStarGround Pos;
        public int P;
        public int C;
        public bool CanMove;
        public Node Parent = null;

        public Node(IAStarGround myPos, Vector2Int targetPos, bool canMove) {
            Pos = myPos;
            P = 0;
            C = GetPathDis(myPos, targetPos);
            CanMove = canMove;
        }

        public int PC => P + C;
    }

    private static int GetPathDis(IAStarGround startPos, Vector2Int targetPos) {
        Vector2Int offset = startPos.GetPos() - targetPos;

        return Mathf.Abs(offset.x) + Mathf.Abs(offset.y);
    }

    private static Stack<IAStarGround> GetPath(Node node) {
        Stack<IAStarGround> path = new Stack<IAStarGround>();
        while (node.Parent != null) {
            path.Push(node.Pos);
            node = node.Parent;
        }
        return path;
    }
}

public interface IAStarGround
{
    Vector2Int GetPos();
    bool CanCross();
}

public class PosData : IAStarGround
{
    private Vector2Int Pos;
    private bool Cross;
        
    public PosData(Vector2Int pos,bool cross)
    {
        Pos = pos;
        Cross = cross;
    }
        
    public Vector2Int GetPos()
    {
        return Pos;
    }

    public bool CanCross()
    {
        return Cross;
    }
}
