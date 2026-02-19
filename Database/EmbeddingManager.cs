using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Tokenizers.HuggingFace.Tokenizer;

namespace SanyaLex.Database
{
    public class EmbeddingManager : IDisposable
    {
        private readonly InferenceSession _session;
        private readonly Tokenizer _tokenizer;

        public EmbeddingManager(string modelPath, string tokenizerJsonPath)
        {
            _session = new InferenceSession(modelPath);
            _tokenizer = Tokenizer.FromFile(tokenizerJsonPath);
        }

        public float[] GetEmbedding(string input)
        {
            var encoding = _tokenizer.Encode(input, true).First();

            long[] ids = encoding.Ids.Select(x => (long)x).ToArray();
            long[] mask = encoding.AttentionMask.Select(x => (long)x).ToArray();
            long[] typeIds = new long[ids.Length];

            // define shape : [BatchSize (1), SequenceLength]
            int[] shape = { 1, ids.Length };

            //create Tensors
            var inputTensor = new DenseTensor<long>(ids, shape);
            var maskTensor = new DenseTensor<long>(mask, shape);
            var typeTensor = new DenseTensor<long>(typeIds, shape);

            //map input for sesson
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor),
                NamedOnnxValue.CreateFromTensor("token_type_ids", typeTensor)
            };

            // 2. Inference ausführen
            using var results = _session.Run(inputs);

            return results.First().AsEnumerable<float>().ToArray();
        }

        public void Dispose()
        {
            _session?.Dispose();
            _tokenizer?.Dispose();
        }


    }
}