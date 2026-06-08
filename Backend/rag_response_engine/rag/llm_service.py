import os
import ollama
from dotenv import load_dotenv

# Robustly find and load the closest .env file climbing up from this directory
current_dir = os.path.dirname(os.path.abspath(__file__))
for _ in range(5):
    env_path = os.path.join(current_dir, ".env")
    if os.path.exists(env_path):
        load_dotenv(env_path)
        break
    parent = os.path.dirname(current_dir)
    if parent == current_dir:
        break
    current_dir = parent

class BaseLLMService:
    def generate(self, prompt: str, system_prompt: str = None) -> str:
        raise NotImplementedError

class OllamaService(BaseLLMService):
    def __init__(self, model_name: str, num_ctx: int = 8192, temperature: float = 0.2):
        self.model_name = model_name
        self.num_ctx = num_ctx
        self.temperature = temperature

    def generate(self, prompt: str, system_prompt: str = None) -> str:
        messages = []
        if system_prompt:
            messages.append({
                "role": "system",
                "content": system_prompt
            })
        messages.append({
            "role": "user",
            "content": prompt
        })
        
        response = ollama.chat(
            model=self.model_name,
            messages=messages,
            options={
                "temperature": self.temperature,
                "num_ctx": self.num_ctx
            }
        )
        return response["message"]["content"]

class OpenAIService(BaseLLMService):
    def __init__(self, model_name: str, temperature: float = 0.2):
        self.model_name = model_name
        self.temperature = temperature

    def generate(self, prompt: str, system_prompt: str = None) -> str:
        try:
            import openai
        except ImportError:
            raise ImportError(
                "The 'openai' library is required to use the OpenAI provider. "
                "Please run `pip install openai` to enable it."
            )
        
        api_key = os.getenv("OPENAI_API_KEY")
        if not api_key:
            raise ValueError("OPENAI_API_KEY environment variable is not set.")
        
        client = openai.OpenAI(api_key=api_key)
        
        messages = []
        if system_prompt:
            messages.append({
                "role": "system",
                "content": system_prompt
            })
        messages.append({
            "role": "user",
            "content": prompt
        })
        
        response = client.chat.completions.create(
            model=self.model_name,
            messages=messages,
            temperature=self.temperature
        )
        return response.choices[0].message.content

class LLMServiceFactory:
    @staticmethod
    def get_service() -> BaseLLMService:
        provider = os.getenv("LLM_PROVIDER", "ollama").lower()
        model = os.getenv("LLM_MODEL", "llama3")
        temperature = float(os.getenv("LLM_TEMPERATURE", "0.2"))
        
        if provider == "ollama":
            num_ctx = int(os.getenv("LLM_NUM_CTX", "8192"))
            return OllamaService(model_name=model, num_ctx=num_ctx, temperature=temperature)
        elif provider == "openai":
            return OpenAIService(model_name=model, temperature=temperature)
        else:
            raise ValueError(f"Unsupported LLM provider: {provider}")

def generate_response(prompt, system_prompt=None):
    if system_prompt is None:
        system_prompt = (
            "You are a professional assistant. "
            "Answer only using the provided "
            "knowledge base context."
        )
    service = LLMServiceFactory.get_service()
    return service.generate(prompt, system_prompt=system_prompt)