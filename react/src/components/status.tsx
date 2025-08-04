import "../App.css";
import React from "react";


function StatusRow({ name, value, symbol = "🟢" }) {
  return (
    <div className="wrapped-label m-10">
      <div className="header">{name}</div>

      <div className="body">

        <div className="status-bar bg-light p-10 rounded">
          {Array.from({ length: value }, (_, i) => (
            <span>{symbol}</span>
          ))}
        </div>

      </div>
    </div>
  )
}

function StatusPanel({ children, name = "Status" }) {

  return (
    <div className="flex-row wrap-on-mobile w-100">
      <div className="card mw-400px grow-4" >

        <div className="card-header">
          Status
        </div>

        {children}

      </div>
    </div>)
}

export {StatusPanel, StatusRow};
