using System;

namespace MegacoolInternal {
    public class EventHandler {
        private Megacool megacool;

        public EventHandler(Megacool megacool) {
            this.megacool = megacool;
        }

        public void LinkClicked(MegacoolLinkClickedEvent e) {
            Action<MegacoolLinkClickedEvent> actionDelegate = megacool.LinkClicked;
            if (actionDelegate != null) {
                actionDelegate(e);
            }
        }

        public void ReceivedShareOpened(MegacoolReceivedShareOpenedEvent e) {
            Action<MegacoolReceivedShareOpenedEvent> actionDelegate = megacool.ReceivedShareOpened;
            if (actionDelegate != null) {
                actionDelegate(e);
            }
        }

        public void SentShareOpened(MegacoolSentShareOpenedEvent e) {
            Action<MegacoolSentShareOpenedEvent> actionDelegate = megacool.SentShareOpened;
            if (actionDelegate != null) {
                actionDelegate(e);
            }
        }

        public void ShareDismissed() {
            SafeInvokeVoidDelegate(megacool.DismissedSharing);
        }

        public void ShareCompleted() {
            SafeInvokeVoidDelegate(megacool.CompletedSharing);
        }

        public void SharePossiblyCompleted() {
            SafeInvokeVoidDelegate(megacool.PossiblyCompletedSharing);
        }

        private void SafeInvokeVoidDelegate(Action action) {
            if (action != null) {
                action();
            }
        }
    }
}
