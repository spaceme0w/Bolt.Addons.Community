﻿using Bolt;
using Bolt.Addons.Community.Utility;
using Bolt.Community.Addons.Utility;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.FlowEvents
{
    [UnitCategory("Events/Editor")]
    [UnitShortTitle("Manual Event")]
    [UnitTitle("Manual Event")]
    public class ManualEvent : MachineEventUnit<EmptyEventArgs>, IGraphElementWithData
    {
        public new class Data : EventUnit<EmptyEventArgs>.Data
        {
            //Tracked per instance of the node.
            public int LastObservedUpdateTicker = 0;

            public Delegate update;
        }

        public override IGraphElementData CreateData()
        {
            return new Data();
        }

        protected override string hookName => EventHooks.Update;

        [UnitHeaderInspectable]
        [UnitButton("TriggerButton")]
        public UnitButton triggerButton;


        //Tracked class-wide (because it updates outside of the graph scope)
        public int shouldTriggerNextUpdateTicker;

        public void TriggerButton(GraphReference reference)
        {
            //By default, use immediate mode execution.  Even coroutines, which we just have to show a warning for and move on.
            bool immediate = true;

            if (Application.isEditor)
            {
                if (Application.isPlaying)
                {
                    //Coroutines always have to defer in play mode. 
                    if (coroutine)
                        immediate = false;
                    else
                        immediate = EditorState.IsEditorPaused();           //Current thinking is do paused triggers immediately, but defer to the next update when playing.
                }
            }



            if (immediate)
            {
                //In the editor, we just fire immediately.

                Debug.Log("Immediate");
                if (coroutine)
                    Debug.LogWarning("This manual event is marked as a coroutine, but Unity coroutines are only valid during Play mode.  Attempting non-coroutine activation!");
                Flow flow = Flow.New(reference);
                flow.Run(trigger);
            }
            else
            {
                Debug.Log("Deferring");
                shouldTriggerNextUpdateTicker++;        //When run in game, we sync it to the update
            }
        }


        protected override bool ShouldTrigger(Flow flow, EmptyEventArgs args)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (!data.isListening)
            {
                return false;
            }

            if (shouldTriggerNextUpdateTicker > data.LastObservedUpdateTicker)
            {
                data.LastObservedUpdateTicker = shouldTriggerNextUpdateTicker;
                return true;
            }
            else
                return false;
        }
    }
}