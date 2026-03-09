import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, Zap, Shield, Truck } from 'lucide-react';
import { productApi } from '../api/product';
import ProductCard from '../components/ProductCard';
import type { ProductListItem } from '../types';

export default function HomePage() {
  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    productApi
      .getPaged(1, 8)
      .then((res) => {
        setProducts(res.data.items);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <>
      {/* Hero Section */}
      <section className="gradient-hero text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 md:py-32">
          <div className="max-w-2xl">
            <h1 className="text-4xl md:text-6xl font-extrabold leading-tight tracking-tight">
              Gear Up.
              <br />
              <span className="bg-gradient-to-r from-white to-gray-400 bg-clip-text text-transparent">
                Stand Out.
              </span>
            </h1>
            <p className="mt-6 text-lg text-gray-300 leading-relaxed">
              Discover premium products crafted for performance and style.
              SHNGear delivers quality you can trust.
            </p>
            <div className="mt-8 flex flex-wrap gap-4">
              <Link
                to="/products"
                className="inline-flex items-center gap-2 bg-white text-black px-6 py-3 rounded-full font-semibold text-sm hover:bg-gray-100 transition-colors"
              >
                Browse Products
                <ArrowRight className="w-4 h-4" />
              </Link>
              <Link
                to="/register"
                className="inline-flex items-center gap-2 border border-white/30 text-white px-6 py-3 rounded-full font-semibold text-sm hover:bg-white/10 transition-colors"
              >
                Create Account
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Features */}
      <section className="py-16 border-b border-gray-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {[
              {
                icon: Zap,
                title: 'Fast Performance',
                desc: 'Products built for speed and reliability.',
              },
              {
                icon: Shield,
                title: 'Trusted Quality',
                desc: 'Every item passes strict quality checks.',
              },
              {
                icon: Truck,
                title: 'Free Shipping',
                desc: 'Free delivery on orders over $50.',
              },
            ].map((f) => (
              <div
                key={f.title}
                className="flex items-start gap-4 p-6 rounded-2xl hover:bg-gray-50 transition-colors"
              >
                <div className="flex-shrink-0 w-12 h-12 bg-black rounded-xl flex items-center justify-center">
                  <f.icon className="w-5 h-5 text-white" />
                </div>
                <div>
                  <h3 className="font-semibold text-gray-900">{f.title}</h3>
                  <p className="text-sm text-gray-500 mt-1">{f.desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Products Grid */}
      <section className="py-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between mb-10">
            <div>
              <h2 className="text-2xl md:text-3xl font-bold text-gray-900">
                Latest Products
              </h2>
              <p className="text-gray-500 mt-1">
                Fresh arrivals just for you
              </p>
            </div>
            <Link
              to="/products"
              className="hidden sm:inline-flex items-center gap-1 text-sm font-medium text-black hover:underline"
            >
              View All <ArrowRight className="w-4 h-4" />
            </Link>
          </div>

          {loading ? (
            <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
              {Array.from({ length: 8 }).map((_, i) => (
                <div key={i} className="animate-pulse">
                  <div className="aspect-square bg-gray-100 rounded-2xl" />
                  <div className="mt-3 h-3 bg-gray-100 rounded w-1/3" />
                  <div className="mt-2 h-4 bg-gray-100 rounded w-2/3" />
                  <div className="mt-2 h-4 bg-gray-100 rounded w-1/4" />
                </div>
              ))}
            </div>
          ) : products.length > 0 ? (
            <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
              {products.map((p) => (
                <ProductCard key={p.id} product={p} />
              ))}
            </div>
          ) : (
            <div className="text-center py-20">
              <p className="text-gray-400 text-lg">No products available yet.</p>
              <p className="text-gray-300 text-sm mt-2">
                Check back soon or run the backend to seed data.
              </p>
            </div>
          )}

          <div className="mt-8 text-center sm:hidden">
            <Link
              to="/products"
              className="inline-flex items-center gap-1 text-sm font-medium text-black hover:underline"
            >
              View All Products <ArrowRight className="w-4 h-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-gray-50 py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-2xl md:text-3xl font-bold text-gray-900">
            Ready to get started?
          </h2>
          <p className="text-gray-500 mt-3 max-w-md mx-auto">
            Create an account today and get access to exclusive deals and early product drops.
          </p>
          <Link
            to="/register"
            className="mt-6 inline-flex items-center gap-2 gradient-btn text-white px-8 py-3 rounded-full font-semibold text-sm transition-all hover:shadow-xl"
          >
            Sign Up Now <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </section>
    </>
  );
}
