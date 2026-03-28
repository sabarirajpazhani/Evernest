import { useState, useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { 
  Home, 
  MessageSquare, 
  Users, 
  Calendar, 
  User, 
  Settings, 
  LogOut,
  Menu,
  X,
  Bell
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '@/app/store';
import { toggleSidebar, setMobile } from '@/app/slices/uiSlice';
import { Button } from '@/components/ui/Button';

interface LayoutProps {
  children?: React.ReactNode;
}

const Layout = ({ children }: LayoutProps) => {
  const dispatch = useDispatch();
  const { user, logout } = useAuth();
  const { sidebarOpen, isMobile } = useSelector((state: RootState) => state.ui);
  const [notificationsOpen, setNotificationsOpen] = useState(false);

  useEffect(() => {
    const handleResize = () => {
      dispatch(setMobile(window.innerWidth < 768));
    };

    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [dispatch]);

  const sidebarItems = [
    { icon: Home, label: 'Dashboard', path: '/' },
    { icon: MessageSquare, label: 'Chat', path: '/chat' },
    { icon: Users, label: 'Friends', path: '/friends' },
    { icon: Calendar, label: 'Events', path: '/events' },
    { icon: User, label: 'Profile', path: '/profile' },
  ];

  if (user?.role === 'Admin') {
    sidebarItems.push({ icon: Settings, label: 'Admin', path: '/admin' });
  }

  const handleLogout = () => {
    logout();
  };

  return (
    <div className="min-h-screen bg-dark-bg flex">
      {/* Sidebar */}
      <div className={`
        ${isMobile ? 'fixed' : 'relative'} 
        ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'} 
        lg:translate-x-0 z-50 w-64 h-full bg-dark-card border-r border-dark-border transition-transform duration-300
      `}>
        <div className="flex items-center justify-between p-6 border-b border-dark-border">
          <h1 className="text-xl font-bold text-dark-text">Evernest</h1>
          {isMobile && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => dispatch(toggleSidebar())}
            >
              <X size={20} />
            </Button>
          )}
        </div>

        <nav className="p-4">
          <ul className="space-y-2">
            {sidebarItems.map((item) => (
              <li key={item.path}>
                <a
                  href={item.path}
                  className="sidebar-item"
                >
                  <item.icon size={20} />
                  <span>{item.label}</span>
                </a>
              </li>
            ))}
          </ul>
        </nav>

        <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-dark-border">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 rounded-full bg-primary-500 flex items-center justify-center text-white font-semibold">
              {user?.username?.[0]?.toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-dark-text truncate">
                {user?.username}
              </p>
              <p className="text-xs text-dark-muted truncate">
                @{user?.username}
              </p>
            </div>
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={handleLogout}
            className="w-full justify-start"
          >
            <LogOut size={20} className="mr-2" />
            Logout
          </Button>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col lg:ml-0">
        {/* Header */}
        <header className="bg-dark-card border-b border-dark-border px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              {isMobile && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => dispatch(toggleSidebar())}
                >
                  <Menu size={20} />
                </Button>
              )}
              <h2 className="text-lg font-semibold text-dark-text">
                Welcome back, {user?.username}!
              </h2>
            </div>

            <div className="flex items-center gap-4">
              <div className="relative">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setNotificationsOpen(!notificationsOpen)}
                >
                  <Bell size={20} />
                </Button>
                {/* Notification dropdown would go here */}
              </div>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 p-6 overflow-auto">
          <Outlet />
        </main>
      </div>

      {/* Mobile Sidebar Overlay */}
      {isMobile && sidebarOpen && (
        <div
          className="fixed inset-0 bg-black/50 z-40 lg:hidden"
          onClick={() => dispatch(toggleSidebar())}
        />
      )}
    </div>
  );
};

export default Layout;
