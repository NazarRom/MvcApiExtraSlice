using ExtraSliceV2.Extensions;
using ExtraSliceV2.Filters;
using ExtraSliceV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MVCApiExtraSlice.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExtraSliceV2.Controllers
{
    public class CartaController : Controller
    {
        private ServiceRestaurante service;
        private IMemoryCache memoryCache;

        public CartaController(ServiceRestaurante service, IMemoryCache memoryCache)
        {
            this.service = service;
            this.memoryCache = memoryCache;

        }
        //miedo
        [AuthorizeUsuarios]
        public async Task<IActionResult> PerfilUsuario()
        {
            string token = HttpContext.Session.GetString("TOKEN");
            Usuario user = await this.service.GetPerfilUserAsync(token);
            return View(user);
        }


        public async Task<IActionResult> Index()
        {
            List<Categoria> categorias = new();
               categorias = await this.service.GetAllCategoriasAsync();
            foreach (var cat in categorias)
            {
                if (cat.Imagen != null)
                {
                    cat.Imagen = await this.service.GetBlobUriAsync("extrasliceblobs", cat.Imagen);
                }
            }
            return View(categorias);
        }

        ////////////////////////////////////////////////////////////////Vistas Partiales RESTAURANTES

        public async Task<IActionResult> _RestaurantesPartial()
        {
            List<Restaurante> restaurantes = new List<Restaurante>();
            restaurantes = await this.service.GetRestaurantesAsync();
            foreach (var res in restaurantes)
            {
                if (res.Imagen != null)
                {
                    res.Imagen = await this.service.GetBlobUriAsync("extrasliceblobs", res.Imagen);
                }
            }
            return PartialView("_RestaurantesPartial", restaurantes);
        }

        public async Task<IActionResult> _ResaturanteOnCategoria(int idcategoria)
        {
            
            List<Restaurante> restaurantes = new List<Restaurante>();
            restaurantes = await this.service.RestaurantesByCategoriaAsync(idcategoria); ;
            foreach (var res in restaurantes)
            {
                if (res.Imagen != null)
                {
                    res.Imagen = await this.service.GetBlobUriAsync("extrasliceblobs", res.Imagen);
                }
            }
            return PartialView("_ResaturanteOnCategoria", restaurantes);
        }

        public async Task<IActionResult> _RestaurantesByDinero(int dinero)
        {
            
            List<Restaurante> restaurantes = new List<Restaurante>();
            restaurantes = await this.service.RestaurantesByMoneyAsync(dinero);
            foreach (var res in restaurantes)
            {
                if (res.Imagen != null)
                {
                    res.Imagen = await this.service.GetBlobUriAsync("extrasliceblobs", res.Imagen);
                }
            }
            return PartialView("_ResaturanteOnCategoria", restaurantes);
        }

        public async Task<IActionResult> _RestauranteByName(string name)
        {
            List<Restaurante> restaurantes = await this.service.GetRestaurantesAsync();
            //var result = restaurantes.Where(r => r.Nombre_restaurante.Contains(name)).ToList();
            var result = restaurantes.Where(x => x.Nombre_restaurante.StartsWith(name, StringComparison.OrdinalIgnoreCase)).Select(x => x.Nombre_restaurante);
            return Json(result);
        }
        public async Task<IActionResult> _ShowRestauranteByName(string name)
        {
            Restaurante restaurante = await this.service.RestauranteByNameAsync(name);

            if (restaurante.Imagen != null)
            {
                restaurante.Imagen = await this.service.GetBlobUriAsync("extrasliceblobs", restaurante.Imagen);
            }

            return PartialView("_ShowRestauranteByName", restaurante);
        }

        /////////////////////////////////////////////////////////////////////////////


        [AuthorizeUsuarios]
        public async Task<IActionResult> CarritoProductos(int? ideliminar)
        {
            //tenemos una coleccion de ids y necesitamos
            //recuperamos los datos de session

            //cambiar el int por modelo de cantidad y el id y así guardarlo
            List<int> idsProductos = HttpContext.Session.GetObject<List<int>>("IdProductos");

            if (idsProductos == null)
            {
                ViewData["MENSAJE"] = "No hay productos";
                return View();
            }
            else
            {
                if (ideliminar != null)
                {
                    //ELIMINAMOS EL ELEMENTO QUE NOS HAN SOLICITADO
                    idsProductos.Remove(ideliminar.Value);
                    if (idsProductos.Count == 0)
                    {
                        HttpContext.Session.Remove("IdProductos");
                    }
                    else
                    {
                        //DEBEMOS ACTUALIZAR DE NUEVO SESSION
                        HttpContext.Session.SetObject("IdProductos", idsProductos);
                    }
                }
                string token = HttpContext.Session.GetString("TOKEN");
                List<Producto> productosSession = await this.service.GetProductosFromSessionAsync(idsProductos, token);
                return View(productosSession);

            }
        }

        [HttpPost]
        [AuthorizeUsuarios]
        public async Task<IActionResult> CarritoProductos(int idcliente, List<int> idproducto, List<int> cantidad)
        {
            string token = HttpContext.Session.GetString("TOKEN");
            List<Producto> productosSession = await this.service.GetProductosFromSessionAsync(idproducto, token);
            string productostring = JsonConvert.SerializeObject(productosSession);

            List<int> prodCantidad = cantidad;
            string prodCanString = JsonConvert.SerializeObject(prodCantidad);

            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            await this.service.SendMailAsync(usuario.Email, productostring, prodCanString);


            await this.service.FinalizarPedidoAsync(idcliente, idproducto, cantidad, token);
            HttpContext.Session.Remove("IdProductos");
            return RedirectToAction("Index");
        }



        [AuthorizeUsuarios]
        public IActionResult Favoritos(int? ideliminar)
        {
            List<Producto> productosFavoritos;
            if (this.memoryCache.Get("FAVORITOS") == null)
            {
                productosFavoritos = new List<Producto>();
            }
            else
            {
                productosFavoritos = this.memoryCache.Get<List<Producto>>("FAVORITOS");
                if (ideliminar != null)
                {
                    //ELIMINAMOS EL ELEMENTO QUE NOS HAN SOLICITADO
                    Producto product = productosFavoritos.FirstOrDefault(p => p.IdProducto == ideliminar);
                    productosFavoritos.Remove(product);
                    if (productosFavoritos.Count == 0)
                    {
                        this.memoryCache.Remove("FAVORITOS");
                        ViewData["mensaje"] = "No hay favoritos";
                    }
                    else
                    {
                        //DEBEMOS ACTUALIZAR DE NUEVO SESSION
                        this.memoryCache.Set("FAVORITOS", productosFavoritos);
                    }

                }


            }
            return View(productosFavoritos);
        }


        public async Task<IActionResult> Restaurante(int idrestaurante, int? idproducto, int? idfavorito)
        {
            if (idfavorito != null)
            {
                List<Producto> productoFavoritos;
                if (this.memoryCache.Get("FAVORITOS") == null)
                {
                    productoFavoritos = new List<Producto>();
                }
                else
                {
                    productoFavoritos = this.memoryCache.Get<List<Producto>>("FAVORITOS");
                }
                //BUSCAMOS PRODUCTO EN BBDD PARA ALMACENARLO EN CACHE
                Producto producto = await this.service.FindProductoAsync(idfavorito.Value);


                Producto productFav = productoFavoritos.FirstOrDefault(p => p.IdProducto == idfavorito);

                if (productFav == null)
                {
                    productoFavoritos.Add(producto);
                    //ALMACENAMOS LOS DATOS EN CACHE
                    this.memoryCache.Set("FAVORITOS", productoFavoritos);
                }

            }

            if (idproducto != null)
            {
                List<int> idsProductos;
                if (HttpContext.Session.GetObject<List<int>>("IdProductos") == null)
                {
                    //creamos la lista para los ids
                    idsProductos = new List<int>();
                }
                else
                {
                    idsProductos = HttpContext.Session.GetObject<List<int>>("IdProductos");
                }
                idsProductos.Add(idproducto.Value);
                HttpContext.Session.SetObject("IdProductos", idsProductos);

            }

            RestauranteProductos restauranteProductos = new RestauranteProductos
            {
                Restaurante = await this.service.GetOneRestauranteAsync(idrestaurante),
                Productos = await this.service.FindProductosByRestauranIdAsync(idrestaurante)
            };

            if (restauranteProductos.Restaurante.Imagen != null)
            {
                restauranteProductos.Restaurante.Imagen = await this.service.GetBlobUriAsync("extrasliceblobs", restauranteProductos.Restaurante.Imagen);
            }

            return View(restauranteProductos);
        }



        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            await this.service.RegisterUserAsync(usuario.Nombre_cliente, usuario.Direccion, usuario.Telefono, usuario.Email, usuario.Password);
            return RedirectToAction("Index");
        }






    }
}
