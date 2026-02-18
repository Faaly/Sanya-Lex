using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;


namespace SanyaLex.Database
{
    public class EmbeddingManager : IDisposable
    {
        private readonly InferenceSession _session;
        private readonly Tokenizer _tokenizer;

        public EmbeddingManager(string modelPath, string vocabPath)
        {
            // Hier laden wir nur die schweren Ressourcen einmalig
            _session = new InferenceSession(modelPath);
            _tokenizer = new BertTokenizer(vocabPath);
        }

        public float[] GetEmbedding(string text)
        {
            // Hier passiert die Arbeit, wenn ein Text reinkommt
            var encoded = _tokenizer.Encode(text);
            var input = encoded.First(); 

            // Diese Daten schicken wir gleich an das ONNX-Modell
            long[] inputIds = input.InputIds;
            long[] attentionMask = input.AttentionMask;
            long[] typeIds = input.TokenTypeIds;

            // Platzhalter: Hier kommt gleich die ONNX-Magie
            return new float[384];}

        public void Dispose() => _session.Dispose();
    }
}