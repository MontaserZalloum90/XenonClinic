import { Outlet } from 'react-router-dom';
import { Header } from '../ui/Header';
import { Footer } from '../ui/Footer';

export function MarketingLayout() {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="flex-1 pt-16">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}

export default MarketingLayout;
