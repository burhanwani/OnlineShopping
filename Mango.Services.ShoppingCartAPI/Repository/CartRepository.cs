using AutoMapper;
using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private IMapper _mapper;

        public CartRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<bool> ApplyCoupon(string userId, string couponCode)
        {
            var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(u => u.UserId == userId);
            cartHeaderFromDb.CouponCode = couponCode;
            _db.CartHeaders.Update(cartHeaderFromDb);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveCoupon(string userId)
        {
            var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(u => u.UserId == userId);
            cartHeaderFromDb.CouponCode = "";
            _db.CartHeaders.Update(cartHeaderFromDb);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ClearCart(string userId)
        {
            var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(u => u.UserId == userId);
            if (cartHeaderFromDb != null)
            {
                _db.CartDetails
                    .RemoveRange(_db.CartDetails.Where(u => u.CartHeaderId == cartHeaderFromDb.CartHeaderId));
                _db.CartHeaders.Remove(cartHeaderFromDb);
                await _db.SaveChangesAsync();
                return true;

            }
            return false;
        }

        // This will be called only from the Mango.Web HomeController api DetailsPost where the cartDto will only contain one product. 
        public async Task<CartDto> CreateUpdateCart(CartDto cartDto)
        {

            Console.WriteLine("createUpdateCart called");
            
            Cart cart = _mapper.Map<Cart>(cartDto);
            //cartHeader and cartDetails
            var prodId = cart.CartDetails.FirstOrDefault().ProductId;
            // check if the product is present in the db or not. IF not, create it. 
            var prodInDb = await _db.Products.FirstOrDefaultAsync(u => u.ProductId == prodId);
            if (prodInDb == null)
            {
                // From the Mango.Web side, we are using Virtual Product within the CartDetails model. 
                // TODO: How would this be in case we dont use the virtual product and use fluent api style models like in this Mango.ShoppingCartApi project ?? 
                _db.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                await _db.SaveChangesAsync();

            }

            // check if the cartHeader is null.
            // TODO:  Check again what AsNoTracking does. 
            var cartHeaderInDb = await _db.CartHeaders.AsNoTracking().
                FirstOrDefaultAsync(c => c.UserId == cartDto.CartHeader.UserId);
            if (cartHeaderInDb == null)
            {
                 
                _db.CartHeaders.Add(cart.CartHeader);
                await _db.SaveChangesAsync();
                // cart.CartHeader will contain the cartHeaderId now generated by Entity Framework. 
                // TODO: Compare this with the method implemented in the course. Does Entity framwork auto-populate the ids after putting it in the database ?? 
                /* https://stackoverflow.com/questions/5212751/how-can-i-retrieve-id-of-inserted-entity-using-entity-framework */
                cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.CartHeaderId;
                // TODO: why is setting the product to null necessary ? It errors out otherwise. 
                cart.CartDetails.FirstOrDefault().Product = null;
                _db.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _db.SaveChangesAsync();
            }
            else
            {
                var cartDetailsInDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync
                    (d => d.CartHeaderId == cartHeaderInDb.CartHeaderId
                        && d.ProductId == cart.CartDetails.FirstOrDefault().ProductId);
                if (cartDetailsInDb == null)
                {
                    //create details
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeaderInDb.CartHeaderId;

                    cart.CartDetails.FirstOrDefault().Product = null;
                    _db.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                    await _db.SaveChangesAsync();
                }
                else
                {
                    //update the count / cart details
                    cart.CartDetails.FirstOrDefault().Product = null;
                    cart.CartDetails.FirstOrDefault().Count += cartDetailsInDb.Count;
                    cart.CartDetails.FirstOrDefault().CartDetailsId = cartDetailsInDb.CartDetailsId;
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartDetailsInDb.CartHeaderId;
                    _db.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                    await _db.SaveChangesAsync();
                }
            }
            return _mapper.Map<CartDto>(cart);
        }    

        

        public async Task<CartDto> GetCartByUserId(string userId)
        {
            Cart cart = new()
            {
                CartHeader = await _db.CartHeaders.FirstOrDefaultAsync(u => u.UserId == userId)
            };

            // TODO: What is the use of the include ? 
            cart.CartDetails = _db.CartDetails
                .Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId).Include(u => u.Product);
            return _mapper.Map<CartDto>(cart);
        }
        
      
        
        public async Task<bool> RemoveFromCart(int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails = await _db.CartDetails
                    .FirstOrDefaultAsync(u => u.CartDetailsId == cartDetailsId);

                int totalCountOfCartItems = _db.CartDetails
                    .Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();

                _db.CartDetails.Remove(cartDetails);
                if (totalCountOfCartItems == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders
                        .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);

                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}