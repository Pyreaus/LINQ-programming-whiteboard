               // [...]  controller class definition up here ^       

        // PUT: api/Offer/Edit/5
        [HttpPut("[controller]/[action]/{id:int}")]
        public async Task<IActionResult<int?>> Edit([FromRoute] int id, [FromBody] CreateUpdateOfferViewModel createUpdateOfferViewModel)
        {   
            if (await offerService.GetById(id) is null or not Offer offer) return NotFound(id); 
          
            Offer offerToUpdate = offerService.GetById(id);                                                            
            offerToUpdate.Caption = createUpdateOfferViewModel.Caption;           //
            offerToUpdate.Description = createUpdateOfferViewModel.Description;  //
            offerToUpdate.ImgPath = createUpdateOfferViewModel.ImgPath;         //
            offerService.Update(offerToUpdate);                                // I should be using AutoMapper here with Ignore() in map configuration 

            return NoContent();
        }
        // DELETE: api/Offer/DeleteOffer/5
        [HttpDelete("[controller]/[action]/{id:int}")]
        public async Task<IActionResult<Offer?,int?>> DeleteOffer([FromRoute] int id)
        {
            if (await offerService.GetById(id) is null or not Offer offer) return NotFound(id); 
          
            Offer offerToDelete = offerService.GetById(id);
            offerService.Delete(offerToDelete);

            return Ok(offerToDelete);
        }

         // GET: api/Offer/GetOffer/5
        [HttpGet("[controller]/[action]/{id:int}")]
        public async Task<IActionResult<int?, OfferViewModel?>> GetOffer([FromRoute] int id)
        {
            if (await offerService.GetById(id) is null or not Offer offer) return NotFound(id); 
            Offer offer = offerService.GetById(id);

            PeopleFinderUser peopleFinderUser = userService.GetById(offer.CreatorId);
            OfferViewModel offerVM = new OfferViewModel
            {
                Id = offer.Id,
                ImgPath = offer.ImgPath,
                Caption = offer.Caption,
                Description = offer.Description,
                Timestamped = offer.Timestamped,
                Email = peopleFinderUser.Email,
                Telephone = peopleFinderUser.Telephone,
                FirstName = peopleFinderUser.FirstName,
                LastName = peopleFinderUser.LastName,
                Photo = bnetUrl + peopleFinderUser.Photo
            };
            
            return Ok(offerVM);
        }



//TODO: create an Upload() controller for file uploads 
