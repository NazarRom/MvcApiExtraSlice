using ExtraSliceV2.Models;
using MVCApiExtraSlice.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MVCApiExtraSlice.Services
{
    public class ServiceRestaurante
    {
        private MediaTypeWithQualityHeaderValue Header;
        private string UrlApi;

        public ServiceRestaurante(IConfiguration configuration)
        {
            this.Header = new MediaTypeWithQualityHeaderValue("application/json");
            this.UrlApi = configuration.GetValue<string>("ApiUrl:ApiCubos");
        }
        //get token
        public async Task<string> GetTokenAsync
        (string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                LoginModel model = new LoginModel
                {
                    UserName = username,
                    Password = password
                };
                string jsonModel = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(jsonModel, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data =
                        await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(data);
                    string token =
                        jsonObject.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }
        //call async con el token
        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "bearer " + token);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }
        //call async sin el token
        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default;
                }
            }
        }
        #region RESTAURANTES
        //get all restaurantes
        public async Task<List<Restaurante>> GetRestaurantesAsync()
        {
            string request = "api/restaurantes/getallrestaurantes";
            List<Restaurante> restaurantes = await this.CallApiAsync<List<Restaurante>>(request);
            return restaurantes;
        }
        //get 1 prestaurante
        public async Task<Restaurante> GetOneRestauranteAsync(int id)
        {
            string request = "api/restaurantes/getonerestaurante/" + id;
            Restaurante restaurante = await this.CallApiAsync<Restaurante>(request);
            return restaurante;
        }
        //restaurantes por categoria
        public async Task<List<Restaurante>> RestaurantesByCategoriaAsync(int id)
        {
            string request = "api/restaurantes/restaurantesbycategoria/" + id;
            List<Restaurante> restaurantes = await this.CallApiAsync<List<Restaurante>>(request);
            return restaurantes;
        }
        //restaurantes por dinero 
        public async Task<List<Restaurante>> RestaurantesByMoneyAsync(int cantidad)
        {
            string request = "api/restaurantes/restaurantesbymoney/" + cantidad;
            List<Restaurante> restaurantes = await this.CallApiAsync<List<Restaurante>>(request);
            return restaurantes;
        }
        public async Task<List<Restaurante>> RestaurantesByNameAsync(string name)
        {
            string request = "api/restaurantes/restaurantesbyname/" + name;
            List<Restaurante> restaurantes = await this.CallApiAsync<List<Restaurante>>(request);
            return restaurantes;
        }
        #endregion

        #region PRODUCTOS
        //obtener todos los productos
        public async Task<List<Producto>> GetAllProductosAsync()
        {
            string request = "api/productos/getallproductos";
            List<Producto> productos = await this.CallApiAsync<List<Producto>>(request);
            return productos;
        }
        //get productos fromn session
        public async Task<List<Producto>> GetProductosFromSessionAsync(List<int> ids, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Productos/getproductosfromsession";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                     ("Authorization", "bearer " + token);
                //tenemos que enviar un objeto JSON
                //nos creamos un objeto de la clase Cubo
                //convertimos el objeto a json
                string compra = JsonConvert.SerializeObject(ids);
                //para enviar datos al servicio se utiliza 
                //la clase SytringContent, donde debemos indicar
                //los datos, de ending  y su tipo
                StringContent content = new StringContent(compra, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Producto>>(responseContent);
                }
                else
                {
                    return null;
                }
            };
        }

        //get productos by restaurant
        public async Task<List<Producto>> ProductosByRestauranIdAsync(int id)
        {
            string request = "api/FindProductosbyrestaurantid/" + id;
            List<Producto> productos = await this.CallApiAsync<List<Producto>>(request);
            return productos;

        }

        //find producto en concreto
        public async Task<Producto> FindProductoAsync(int id)
        {
            string request = "api/FindProducto";
            Producto producto = await this.CallApiAsync<Producto>(request);
            return producto;
        }
        #endregion

        #region USUARIOS
        public async Task RegisterUserAsync(string nombre, string direccion, string telefono, string email, string pass)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Usuarios/RegisterUser";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                //tenemos que enviar un objeto JSON
                //nos creamos un objeto de la clase Hospital
                Usuario usuario = new Usuario
                {
                    Nombre_cliente = nombre,
                    Direccion = direccion,
                    Telefono = telefono,
                    Email = email,
                    Password = pass
                    
                };
                //convertimos el objeto a json
                string json = JsonConvert.SerializeObject(usuario);
                //para enviar datos al servicio se utiliza 
                //la clase SytringContent, donde debemos indicar
                //los datos, de ending  y su tipo
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }

        //METODO PROTEGIDO PARA RECUPERAR EL PERFIL
        public async Task<Usuario> GetPerfilUserAsync
            (string token)
        {
            string request = "api/usuarios/perfiluser";
            Usuario empleado = await
                this.CallApiAsync<Usuario>(request, token);
            return empleado;
        }
        #endregion

        #region CARTA
        //finalizar pedido
        public async Task FinalizarPedidoAsync(int idcliente, List<int> idsproducto, List<int> cantidad, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Productos/getproductosfromsession";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                     ("Authorization", "bearer " + token);
                //tenemos que enviar un objeto JSON
                //nos creamos un objeto de la clase Cubo
                //convertimos el objeto a json
                PedidoFinal pedido = new PedidoFinal
                {
                    IdCliente = idcliente,
                    IdsProducto = idsproducto,
                    Cantidad = cantidad
                };
                string json = JsonConvert.SerializeObject(pedido);
                //para enviar datos al servicio se utiliza 
                //la clase SytringContent, donde debemos indicar
                //los datos, de ending  y su tipo
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                
            };
        }

        public async Task<List<Categoria>> GetAllCategoriasAsync()
        {
            string request = "api/Carta/GetAllCategorias";
            List<Categoria> categorias = await this.CallApiAsync<List<Categoria>>(request);
            return categorias;
        }

        #endregion
    }
}

