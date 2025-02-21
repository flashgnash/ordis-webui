import "../App.css";
import React from "react";

function FlexRow({ children, wrapOnMobile = false }) {
  let extraClass = "";
  if(wrapOnMobile === true){
    extraClass = "wrap-on-mobile";
  }
  
  return (
    <div className={`flex-row w-100 ${extraClass}`}>
      {children}
    </div>
  )
}

function Banner({ values }) {
  const values_array = values.split(",");

  return (
    <div className="flex-row">
      {values_array.map((item, _) => (
        <div className="bg-dark w-100 text-title">{item}</div>
      ))}


    </div>
  )

}


function SolidCard({ title, body }) {
  return (

    <div className="card w-100 m-10">
      <div className="card-header">{title}</div>
      <div className="card-body">
        <div className="m-bottom-10">{body}</div>
      </div>
    </div>

  )
}

export {SolidCard, Banner, FlexRow}
