import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Home from './HomePage.tsx'
import './index.css'
import Room from './RoomPage.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
      <Route path="/room/:code" element={<Room />} />
    </Routes>
    </BrowserRouter>
  </StrictMode>,
)
