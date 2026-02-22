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

            const int fixedLength = 128;

            long[] ids = new long[fixedLength];
            long[] mask = new long[fixedLength];
            long[] typeIds = new long[fixedLength];

            int count = Math.Min(encoding.Ids.Count, fixedLength);
            for (int i = 0; i < count; i++)
            {
                ids[i] = (long)encoding.Ids[i];
                if (i < encoding.AttentionMask.Count)
                {
                    mask[i] = (long)encoding.AttentionMask[i];
                }
                else
                {
                    mask[i] = 1L;
                }
            }

            int[] shape = { 1, fixedLength };
            try
            {
                DenseTensor<long> inputTensor;
                try
                {
                    inputTensor = new DenseTensor<long>(ids, shape);
                }
                catch (Exception ex)
                {
                    throw new Exception($"ERROR while creating inputTensor: {ex.Message}");
                }

                var maskTensor = new DenseTensor<long>(mask, shape);
                var typeTensor = new DenseTensor<long>(typeIds, shape);

                //map input for sesson
                var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor),
            };
                //Inference
                using var results = _session.Run(inputs);

                var outputValue = results.First();
                var outputTensor = outputValue.AsTensor<float>();
                var firstOutput = results.First().AsTensor<float>();

                int dimensions = firstOutput.Dimensions[2];
                float[] embedding = new float[dimensions];

                for (int i = 0; i < dimensions; i++)
                {
                    embedding[i] = firstOutput[0, 0, i];
                }

                float[] normalizedEmbedding = Normalize(embedding);

                return normalizedEmbedding;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[INTERNAL ERROR] {ex.Message}");
                throw;
            }
        }

        public float[] Normalize(float[] vector)
        {
            float sum = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += vector[i] * vector[i];
            }
            float length = MathF.Sqrt(sum);

            if (length > 1e-10f)
            {
                for (int i = 0; i < vector.Length; i++) vector[i] /= length;
            }
            return vector;
        }

        public void Dispose()
        {
            _session?.Dispose();
            _tokenizer?.Dispose();
        }


    }
}