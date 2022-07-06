using UnityEngine.UIElements;
using Halluvision.BehaviourTree;

namespace Halluvision.BehaviourTree
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }

        public SplitView()
        {

        }
    }
}