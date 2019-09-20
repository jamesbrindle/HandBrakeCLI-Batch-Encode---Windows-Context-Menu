using System.Diagnostics;

namespace HandBrakeCLIBatchEncode
{
    public interface IBatchEncoder
    {
        void ProcessBatch_OutputDataReceived<T>(object sender, DataReceivedEventArgs e);
        void ProcessBatch_ErrorDataReceived<T>(object sender, DataReceivedEventArgs e);
        void WriteBatchOutput<T>(string output);
        void WriteOutputToFileOption<T>();
    }
}
