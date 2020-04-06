using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegacoolScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Megacool.Instance.ReceivedShareOpened += (MegacoolReceivedShareOpenedEvent megacoolEvent) =>
        {
            Debug.Log("Got event: " + megacoolEvent);
            if (megacoolEvent.IsFirstSession)
            {
                // This device has received a share and installed the
                // app for the first time
                Debug.Log("Installed from a referral from " + megacoolEvent.SenderUserId);
            }
        };
        Megacool.Instance.SentShareOpened += (MegacoolSentShareOpenedEvent megacoolEvent) =>
        {
            Debug.Log("Got event: " + megacoolEvent);
            if (megacoolEvent.IsFirstSession)
            {
                // A share sent from this device has been opened, and
                // the receiver installed the app for the first time
                Debug.Log(megacoolEvent.ReceiverUserId + " installed the app from our referral");
            }
        };
        // Initialize the Megacool SDK. The callbacks must be
        // registered before this.

        Debug.Log("Set Megacool Debug to true");
        Megacool.Debug = true;

        Debug.Log("Megacool.Instance ='" + Megacool.Instance + "'");

        Debug.Log("Starting Megacool");
        Megacool.Instance.Start();
    }


    public void ShareWithMegacool()
    {
        Debug.Log("ShareWithMegacool Megacool");
        Megacool.Instance.Share(new MegacoolShareConfig() { Message = "Boo there" });
    }

}
