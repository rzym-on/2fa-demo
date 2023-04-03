import { useEffect } from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import jwt_decode from 'jwt-decode';
import useUserStore from './stores/user';

function App() {
  const { name, setName } = useUserStore((state) => state);

  function getToken() {
    const localToken = localStorage.getItem('token');
    return localToken ? JSON.parse(localToken) : null;
  }

  useEffect(() => {
    const token = getToken();
    if (!token) return;
    const decoded = jwt_decode(token) as { username: string };
    setName(decoded.username);
  }, [setName]);

  function logout() {
    localStorage.removeItem('token');
    setName('');
    window.location.reload();
  }

  return (
    <div className="container-fluid text-center">
      <ToastContainer position="bottom-right" theme="dark" />
      <div className="row">
        <div className="col-4 router-dashboard view-col">
          <div className="card">
            <div className="card-body">
              <h5 className="card-title h1">
                {name?.length > 0 ? `🔑${name}` : '👤Gal anonim'}
              </h5>
              <p className="card-text" />
            </div>
          </div>
          <div className="list-group nav-list">
            <NavLink to="" className="list-group-item list-group-item-action">
              🌦️ Pogoda
            </NavLink>
            {name?.length > 0 ? (
              <button
                type="button"
                className="list-group-item list-group-item-action"
                onClick={logout}
              >
                🔓 Wyloguj
              </button>
            ) : (
              <>
                <NavLink
                  to="login"
                  className="list-group-item list-group-item-action"
                >
                  🔐 Zaloguj
                </NavLink>
                <NavLink
                  to="register"
                  className="list-group-item list-group-item-action"
                >
                  📝 Zarejestruj
                </NavLink>
              </>
            )}
          </div>
        </div>
        <div className="col view-col">
          <div className="container-fluid text-center">
            <Outlet />
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;
