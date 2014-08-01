using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;
using System.Collections;


namespace GetLDAPUsers
{
    public class myReverserClass : IComparer
    {

        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        public int Compare(object x, object y)
        {
            int answer = Comparer.Default.Compare(
               ((TreeNode)x).Name, ((TreeNode)y).Name);
            return answer;
        }

    }
}
