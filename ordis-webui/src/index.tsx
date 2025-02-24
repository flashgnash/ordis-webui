import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';

import App from './App.tsx';


import CharacterSheet from "./pages/character_sheet.tsx"
import RollPanel from "./pages/rolls.tsx"


import reportWebVitals from './reportWebVitals';
import { FlexRow } from "./components/common.tsx"
import Sidenav from "./components/sidenav.tsx";
import {
  BrowserRouter as Router,
  Routes,
  Route,
} from "react-router-dom";

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <Router>
    <FlexRow>
      <Sidenav />
      <div className="App">

        <Routes>
          <Route path="/" element={<App />} />
          <Route path="/character_sheet" element={<CharacterSheet />} />
          <Route path="/rolls" element={<RollPanel />} />
          <Route path="/map" element={<App />} />
          <Route path="/images" element={<App />} />


        </Routes>
      </div>
    </FlexRow>
  </Router>

);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
