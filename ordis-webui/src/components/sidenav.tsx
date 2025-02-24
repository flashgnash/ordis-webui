
import React from "react";
import { Link } from 'react-router-dom';
import {FlexRow} from '../components/common.tsx'
function Sidenav() {
  return (
    <div className="sidebar">
        <FlexRow>
          <span className="sidebar-title"> Menu </span>
          <button className="sidebar-close">{"<"}</button>
        </FlexRow>
        <Link to="/"> Home</Link>
        <Link to="/rolls"> Rolls</Link>
        <Link to="/character_sheet">Sheet</Link>      
        <Link to="/skill_tree">Skills</Link>      
        <Link to="/map">Map</Link>
        <Link to="/images">Images</Link>
    </div>
  );
}

export default Sidenav;
