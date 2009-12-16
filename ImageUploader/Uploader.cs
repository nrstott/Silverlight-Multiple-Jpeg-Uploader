using System;
using System.IO;
using System.Net;

namespace ImageUploader
{
	public class Uploader: IUploader
	{
		public event EventHandler<ProgressEventArgs> Progress;

		public int ChunkSize { get; set; }

		public void Upload(Uri uri, Stream content)
		{
			OnProgress(0);

			try
			{
				var client = new WebClient();
				client.OpenWriteCompleted += (s, args) =>
				                             	{
				                             		var writeStream = args.Result;
				                             		var totalLength = content.Length;
				                             		var uploadedByteCount = 0;
				                             		byte[] chunk;

				                             		while (uploadedByteCount < totalLength)
				                             		{
				                             			chunk =
				                             				new byte[
				                             					uploadedByteCount + ChunkSize < totalLength
				                             						? ChunkSize
				                             						: totalLength - uploadedByteCount];
				                             			writeStream.Write(chunk, 0, chunk.Length);
				                             			uploadedByteCount += chunk.Length;
				                             			OnProgress(uploadedByteCount/(decimal) totalLength);
				                             		}
				                             	};
				client.OpenWriteAsync(uri, "post");
			} catch (Exception ex)
			{
				throw new UploadException("Upload failed", ex);
			}

		}

		private void OnProgress(decimal i)
		{
			if (Progress != null)
				Progress(this, new ProgressEventArgs());
		}
	}

	public interface IUploader
	{
		event EventHandler<ProgressEventArgs> Progress;

		void Upload(Uri uri, Stream content);
	}

	public class ProgressEventArgs: EventArgs
	{
		private readonly decimal percentComplete;

		public ProgressEventArgs(): this(0)
		{
		}

		public ProgressEventArgs(decimal percentComplete)
		{
			this.percentComplete = percentComplete;
		}

		public decimal PercentComplete
		{
			get { return percentComplete; }
		}
	}

	public class UploadException: IOException
	{
		public UploadException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}