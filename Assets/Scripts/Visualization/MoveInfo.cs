namespace RookWorks.Visualization
{
    public struct MoveInfo
    {
        public int Depth;
        public int MultiPv;
        public int Score;
        public string Move;

        public MoveInfo(int depth, int multpv, int score, string move)
        {
            Depth = depth;
            MultiPv = multpv;
            Score = score;
            Move = move;
        }
    }
}
