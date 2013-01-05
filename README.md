Twilio-Net
==========

Twilio-Net is a set of .Net components for integrating with the Twilio call solution (www.twilio.com).


Twilio.Activities
-----------------

Contains a set of Windows Workflow Foundation activities that can be integrated into a workflow to produce TwiML output. This allows call flow to be designed visually using Windows Workflow Foundation when a workflow is properly exposed on a HTTP endpoint.

To expose a workflow using these activities implement TwilioHttpHandler and expose it on a URL. Extending it from a .ashx is a simple way to accomplish it.