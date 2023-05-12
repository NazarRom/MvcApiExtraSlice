namespace MVCApiExtraSlice.Models
{
    public class PedidoFinal
    {
        public int IdCliente { get; set; }
        public List<int> IdsProducto { get; set; }
        public List<int> Cantidad { get; set; }
    }
}
