
import React from "react";
import { Link, useLocation } from 'react-router-dom';
import { FlexRow } from '../components/common.tsx'


function NavLink({to,icon,label}) {

  const location = useLocation();

  
  return (
    <Link 
      to={to}
      className={location.pathname === to ? "active" : ""} 
    >
    {icon} <span className="nav-label">{label}</span>
    </Link>)
}

function Sidenav() {

  function ToggleVisibility() {
    const sidebar = document.querySelector(".sidebar");
    const button = document.querySelector(".sidebar-toggle");

    const hidden = sidebar.classList.toggle("sidebar-hidden");
    button.textContent = hidden ? ">" : "<";
  }

  return (
    <div className="sidebar sidebar-hidden">
      <FlexRow>
        <span className="sidebar-title"> Menu </span>
        <button className="sidebar-toggle" onClick={ToggleVisibility}>{">"}</button>
      </FlexRow>

      <NavLink to="/character_sheet" icon="📰" label="Sheet"/>
      <NavLink to="/rolls" icon="🎲" label="Rolls"/>
      <NavLink to="/skill_tree" icon="🌳" label="Skills"/>
      <NavLink to="/map" icon="🗺️" label="Map"/>
      <NavLink to="/images" icon="🖼️" label="Images"/>
    </div>
  );
}

export default Sidenav;
