// Modelo utilizado para representar erros no sistema (ex: em páginas de erro)
// Permite mostrar o ID do pedido atual e verificar se ele existe

namespace SequeMusic.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; } // ID do pedido HTTP que gerou o erro

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId); 
        // Indica se o ID do pedido deve ser mostrado (se não for nulo ou vazio)
    }
}