using System;
using Uptred.YouTube;

namespace Uptred.YouTube
{
    [Serializable]
	public class YouTubeUploadTask : UploadTask
    {
		public override string Provider {
			get {
				return "YouTube";
			}
		}

		public string Url = null;
        public YouTubeMetadata Meta = new YouTubeMetadata();

		public override string ToString ()
		{
			return base.ToString () + 
				"Url: " + (Url ?? "null") + "\n" +
				Meta.ToJson ();
		}
    }
}