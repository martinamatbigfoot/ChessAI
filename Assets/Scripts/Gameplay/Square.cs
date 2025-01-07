using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RookWorks
{
    public class Square 
    {
        public int rank;
        public int file;

        public Square(int rank, int file){
            this.rank = rank;
            this.file = file;
        }

        public override string ToString(){
            char fileLetter = (char)(rank + 65);
            return $"{fileLetter}{file + 1}";
        }
    }
}
