using System.Collections.Generic;

namespace ty_game_lib
{
    public class OneSideBlockNote
    {
        private Round A;
        
        private TwoDVectorLine Line;


        public OneSideBlockNote(Round a, TwoDVectorLine line)
        {
            A = a;
            
            Line = line;
        }

        public void CutBy(IEnumerable<OneSideBlockNote> smOthers)
        {
            var shapes = new List<IShape>();

            foreach (var sideBlockNote in smOthers)
            {
                
                
                    
                
                
            }
        }
        
        
    }
}