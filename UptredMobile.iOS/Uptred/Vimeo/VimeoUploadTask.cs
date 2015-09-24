using System;
using System.Collections.Generic;
using Uptred.Vimeo;

namespace Uptred.Vimeo
{
    [Serializable]
    public class VimeoUploadTask : UploadTask
    {
		public override string Provider {
			get {
				return "Vimeo";
			}
		}

		public Ticket Ticket;
        public string VideoId = null;
        public VimeoMetadata Meta = new VimeoMetadata();

		public override string ToString ()
		{
			return base.ToString () + 
				"Ticket: " + (Ticket != null ? Ticket.ToString() : "null") + "\n" +
				Meta.ToString ();
		}
    }
}